using System.Collections.Generic;
using UnityEngine;
using Blizzard.Inventory;
using Zenject;
using ModestTree;
using Blizzard.Utilities;
using Blizzard.Environment;
using Blizzard.Player;
using UnityEngine.InputSystem;


namespace Blizzard.UI
{
    /// <summary>
    /// Handles inventory UI & other inventory interactions
    /// </summary>
    public class InventoryUI : UIBase
    {
        [Header("References")]
        [SerializeField] InventorySlotCtrl _inventorySlotPrefab;
        [SerializeField] Transform _inventorySlotParent;
        [Header("Config")]
        [SerializeField] float _itemDropDistance = 1;

        [Inject] InventoryService _inventoryService;
        [Inject] PlayerService _playerService;
        [Inject] InputService _inputService;
        [Inject] EnvPrefabService _envPrefabService;

        private List<InventorySlotCtrl> _uiSlots = new List<InventorySlotCtrl>();
        /// <summary>
        /// Index of selected slot in _inventoryService.inventorySlots (and thus also in _uiSlots)
        /// </summary>
        private int _selectedSlotIndex;
        
        private bool _setup = false;

        public override void Setup(object args)
        {
            if (!_setup) InitSlots();
            else RefreshAllSlots();
            

            _inventoryService.OnInventoryModified += RefreshSlot;

            // -- Bind input --

            // Select slot with number key
            _inputService.inputActions.Player.NumberKey.performed += OnInputNumberKey;

            // Drop item
            _inputService.inputActions.Player.DropItem.performed += OnInputDropItem;
        }

        public override void Close(bool destroy = true)
        {
            _inventoryService.OnInventoryModified -= RefreshSlot;


            // Unbind Input
            _inputService.inputActions.Player.NumberKey.performed -= OnInputNumberKey;
            _inputService.inputActions.Player.DropItem.performed -= OnInputDropItem;

            // Can't have equipped item if inventory UI closed
            _inventoryService.UnequipItem();

            base.Close(destroy);
        }


        // -- INPUT --
        private void OnInputNumberKey(InputAction.CallbackContext ctx)
        {
            SetSelectedSlot((int)ctx.ReadValue<float>() - 1);
        }

        private void OnInputDropItem(InputAction.CallbackContext _)
        {
            DropSelectedItem();
        }
        // --


        private void InitSlots()
        {
            foreach (InventorySlot slot in _inventoryService.inventorySlots)
            {
                int slotIndex = _uiSlots.Count;

                InventorySlotCtrl uiSlot = Instantiate(_inventorySlotPrefab, _inventorySlotParent);
                uiSlot.transform.SetAsLastSibling();
                uiSlot.LinkedSetup(slot);
                uiSlot.SetSelected(false); // Slots are by default not selected
                uiSlot.slotButton.onClick.AddListener(() => SetSelectedSlot(slotIndex)); // Select this slot on click

                _uiSlots.Add(uiSlot);
            }

            _uiSlots[0].SetSelected(true); // Set first slot as selected initially
            _selectedSlotIndex = 0;
        }

        /// <summary>
        /// Refresh displayed contents of specified slot, re-syncing it with the inventory
        /// </summary>
        /// <param name="index">Index of slot to refresh</param>
        private void RefreshSlot(int index)
        {
            Assert.That(0 <= index && index < _uiSlots.Count, $"Given index ({index}) out of range! Slot count: {_uiSlots.Count}");

            // (re-setup now redundant since slot is just linked)
            //_uiSlots[index].Setup(_inventoryService.inventorySlots[index].Item,
            //                      _inventoryService.inventorySlots[index].Amount);

            if (_selectedSlotIndex == index)
            {
                // Re-equip this slot (unequip and equip)
                // I.e. if item equipped and then removed, will be unequipped
                //   And if slot selected and item added, will be equipped
                _inventoryService.UnequipItem();
                _inventoryService.EquipItem(index);
            }
        }

        /// <summary>
        /// Refresh displayed contents of all slots
        /// </summary>
        private void RefreshAllSlots()
        {
            for (int i = 0; i < _uiSlots.Count; ++i)
            {
                InventorySlot correspondingSlot = _inventoryService.inventorySlots[i];
                _uiSlots[i].Setup(correspondingSlot.Item, correspondingSlot.Amount);
            }
        }

        /// <summary>
        /// Set currently selected slot to new value, has no effect if given selection is identical to current selection
        /// </summary>
        /// <param name="selection"></param>
        private void SetSelectedSlot(int selection)
        {
            if (selection == _selectedSlotIndex) return; // Already selected

            Assert.That(0 <= selection && selection < _uiSlots.Count, $"Given selection ({selection}) is out of range! Slot count: ${_uiSlots.Count}");

            _uiSlots[_selectedSlotIndex].SetSelected(false);
            _uiSlots[selection].SetSelected(true);

            _selectedSlotIndex = selection;
            _inventoryService.EquipItem(selection);
        }

        private void DropSelectedItem()
        {
            ItemData itemToDrop = _inventoryService.inventorySlots[_selectedSlotIndex].Item;
            if (itemToDrop == null) return; // No item to drop from selected slot

            // Attempt drop
            int amountRemoved = _inventoryService.TryRemoveItemAt(_selectedSlotIndex, 1);

            if (amountRemoved == 0) return; // Items wasn't successfully removed, do not drop

            InventoryServiceExtensions.DropItem(_envPrefabService, _playerService, itemToDrop, amountRemoved);
        }
    }
}

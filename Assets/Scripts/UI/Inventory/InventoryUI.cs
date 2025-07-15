using System.Collections.Generic;
using UnityEngine;
using Blizzard.Inventory;
using Zenject;
using ModestTree;
using Blizzard.Utilities;
using Blizzard.Environment;
using Blizzard.Player;


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

        public override void Setup(object args)
        {
            InitSlots();

            _inventoryService.OnInventoryModified += RefreshSlot;

            // -- Setup input --

            // Select slot with number key
            _inputService.inputActions.Player.NumberKey.performed += (ctx) =>
            {
                SetSelectedSlot((int)ctx.ReadValue<float>() - 1);
            };

            // Drop item
            _inputService.inputActions.Player.DropItem.performed += (_) => DropSelectedItem();
        }

        private void InitSlots()
        {
            foreach (InventorySlot slot in _inventoryService.inventorySlots)
            {
                InventorySlotCtrl uiSlot = Instantiate(_inventorySlotPrefab, _inventorySlotParent);
                uiSlot.transform.SetAsLastSibling();
                uiSlot.Setup(slot.item, slot.amount);
                uiSlot.SetSelected(false); // Slots are by default not selected

                _uiSlots.Add(uiSlot);
            }

            _uiSlots[0].SetSelected(true); // Set first slot as selected initially
            _selectedSlotIndex = 0;
        }

        /// <summary>
        /// Refresh displayed contents of specified slot
        /// </summary>
        /// <param name="index">Index of slot to refresh</param>
        private void RefreshSlot(int index)
        {
            Assert.That(0 <= index && index < _uiSlots.Count, $"Given index ({index}) out of range! Slot count: {_uiSlots.Count}");

            _uiSlots[index].Setup(_inventoryService.inventorySlots[index].item,
                                  _inventoryService.inventorySlots[index].amount);
        }

        /// <summary>
        /// Refresh displayed contents of all slots
        /// </summary>
        private void RefreshAllSlots()
        {
            for (int i = 0; i < _uiSlots.Count; ++i)
            {
                InventorySlot correspondingSlot = _inventoryService.inventorySlots[i];
                _uiSlots[i].Setup(correspondingSlot.item, correspondingSlot.amount);
            }
        }

        /// <summary>
        /// Set currently selected slot to new value, has no effect if given selection is identical to current selection
        /// </summary>
        /// <param name="selection"></param>
        private void SetSelectedSlot(int selection)
        {
            if (selection == _selectedSlotIndex) return; // Already selected

            Assert.That(0 <= selection && selection < _uiSlots.Count, $"Given zselection ({selection}) is out of range! Slot count: ${_uiSlots.Count}");

            _uiSlots[_selectedSlotIndex].SetSelected(false);
            _uiSlots[selection].SetSelected(true);

            // TODO: Logic for on item selection (e.g. tool is equipped, building is ready to build)

            _selectedSlotIndex = selection;
            _inventoryService.EquipItem(selection);
        }

        private void DropSelectedItem()
        {
            ItemData itemToDrop = _inventoryService.inventorySlots[_selectedSlotIndex].item;
            if (itemToDrop == null) return; // No item to drop from selected slot

            // Attempt drop
            int amountRemoved = _inventoryService.TryRemoveItemAt(_selectedSlotIndex, 1);

            if (amountRemoved == 0) return; // Items wasn't successfully removed, do not drop

            ItemDrop dropObj = _envPrefabService.InstantiatePrefab("item_drop").GetComponent<ItemDrop>();

            Vector2 plrFacingDirection = _playerService.GetFacingDirection();
            dropObj.transform.position = _playerService.playerCtrl.transform.position + 
                                        new Vector3(plrFacingDirection.x, plrFacingDirection.y, 0) * _itemDropDistance;

            dropObj.Setup(new ItemAmountPair { item = itemToDrop, amount = amountRemoved }); // (amountRemoved should be one, failsafe regardless)
        }
    }
}

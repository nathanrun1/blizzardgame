using System;
using System.Collections.Generic;
using Blizzard.Input;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Zenject;
using Sirenix.OdinInspector;
using Blizzard.Inventory;
using Blizzard.Utilities;
using Blizzard.Utilities.Logging;
using Blizzard.Player;
using Blizzard.UI.Core;
using UnityEngine.UI;

namespace Blizzard.UI.Inventory
{
    /// <summary>
    /// Handles inventory UI & other inventory interactions
    /// </summary>
    public class InventoryUI : UIBase
    {
        [Header("References")] 
        [SerializeField] private InventorySlotCtrl _inventorySlotPrefab;
        [SerializeField] private Transform _inventorySlotSelectorPrefab;

        [SerializeField] private Transform _inventorySlotParent;

        [Inject] private InventoryService _inventoryService;
        [Inject] private PlayerService _playerService;
        [Inject] private InputService _inputService;
        [Inject] private EnvPrefabService _envPrefabService;
        [Inject] private DiContainer _diContainer;

        private List<InventorySlotCtrl> _uiSlots = new();

        /// <summary>
        /// Visual indicator of selected slot
        /// </summary>
        private Transform _slotSelector;
        /// <summary>
        /// Index of selected slot in _inventoryService.inventorySlots (and thus also in _uiSlots)
        /// </summary>
        private int _selectedSlotIndex = -1;

        private bool _setup = false;

        private void Awake()
        {
            _slotSelector = Instantiate(_inventorySlotSelectorPrefab, _inventorySlotParent);
            _slotSelector.gameObject.SetActive(true);
        }

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

            _setup = true;
        }

        public override void Close()
        {
            _inventoryService.OnInventoryModified -= RefreshSlot;

            // Unbind Input
            _inputService.inputActions.Player.NumberKey.performed -= OnInputNumberKey;
            _inputService.inputActions.Player.DropItem.performed -= OnInputDropItem;

            // Can't have equipped item if inventory UI closed
            _inventoryService.UnequipItem();

            base.Close();
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
            foreach (var slot in _inventoryService.inventorySlots)
            {
                var slotIndex = _uiSlots.Count;

                var uiSlot =
                    _diContainer.InstantiatePrefabForComponent<InventorySlotCtrl>(_inventorySlotPrefab,
                        _inventorySlotParent);
                uiSlot.transform.SetAsLastSibling();
                uiSlot.LinkedSetup(slot, true, true);
                uiSlot.SetSelected(false); // Slots are by default not selected
                uiSlot.SetMoveInEnabled(true); // Move in/out enabled for inventory
                uiSlot.SetMoveOutEnabled(true); // ^^
                uiSlot.slotButton.onClick.AddListener(() => SetSelectedSlot(slotIndex)); // Select this slot on click

                _uiSlots.Add(uiSlot);
            }

            _slotSelector.SetAsLastSibling(); // Ensure slot selector rendered above all slots
            LayoutRebuilder.ForceRebuildLayoutImmediate(_inventorySlotParent.transform as RectTransform);
            SetSelectedSlot(0);
        }

        /// <summary>
        /// Refresh equip status of given inventory slot, if is selected
        /// </summary>
        /// <param name="index">Index of slot to refresh</param>
        private void RefreshSlot(int index)
        {
            Assert.IsTrue(0 <= index && index < _uiSlots.Count,
                $"Given index ({index}) out of range! Slot count: {_uiSlots.Count}");

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
            for (var i = 0; i < _uiSlots.Count; ++i)
            {
                var correspondingSlot = _inventoryService.inventorySlots[i];
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

            Assert.IsTrue(0 <= selection && selection < _uiSlots.Count,
                $"Given selection ({selection}) is out of range! Slot count: ${_uiSlots.Count}");

            _slotSelector.transform.localPosition = _uiSlots[selection].transform.localPosition;
            BLog.Log("InventoryUI", $"Set slot selector position to {_slotSelector.transform.position}");
            
            _selectedSlotIndex = selection;
            _inventoryService.EquipItem(selection);
        }

        private void DropSelectedItem()
        {
            var itemToDrop = _inventoryService.inventorySlots[_selectedSlotIndex].Item;
            if (itemToDrop == null) return; // No item to drop from selected slot

            // Attempt drop
            var amountRemoved = _inventoryService.TryRemoveItemAt(_selectedSlotIndex, 1);

            if (amountRemoved == 0) return; // Items aren't successfully removed, do not drop

            InventoryServiceExtensions.DropItem(_envPrefabService, _playerService, itemToDrop, amountRemoved);
        }

        [Button]
        private void TransferItems(int amount, int srcIndex, int destIndex)
        {
            Assert.IsTrue(0 <= srcIndex && srcIndex < _uiSlots.Count, $"Invalid srcIndex: {srcIndex}");
            Assert.IsTrue(0 <= destIndex && destIndex < _uiSlots.Count, $"Invalid destIndex: {destIndex}");
            var srcSlot = _uiSlots[srcIndex];
            var destSlot = _uiSlots[destIndex];

            var amntMoved = srcSlot.TryMoveItemsOut(destSlot, amount);
            BLog.Log($"Transferred {amntMoved} items!");
        }
    }
}
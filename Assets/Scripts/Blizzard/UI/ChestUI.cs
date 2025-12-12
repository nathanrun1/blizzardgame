using System;
using System.Collections.Generic;
using Blizzard.Input;
using UnityEngine;
using Zenject;
using Blizzard.Inventory;
using Blizzard.Inventory.Crafting;
using Blizzard.Obstacles.Concrete;
using Blizzard.UI.Core;
using Blizzard.UI.Inventory;
using Blizzard.Utilities.Logging;
using UnityEngine.InputSystem;

namespace Blizzard.UI
{
    public class ChestUI : UIBase
    {
        [Inject] private DiContainer _diContainer;
        [Inject] private InputService _inputService;

        public struct Args
        {
            public List<InventorySlot> slots;
        }
        
        [Header("References")]
        [SerializeField] private InventorySlotCtrl _inventorySlotPrefab;
        [SerializeField] private Transform _slotParent;

        private List<InventorySlot> _linkedSlots;
        private List<InventorySlotCtrl> _uiSlots = new();

        public override void Setup(object args)
        {
            Args chestArgs;
            try
            {
                chestArgs = (Args)args;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Incorrect argument type given!");
            }

            _linkedSlots = chestArgs.slots;
            SetupUISlots();
            
            _inputService.inputActions.UI.Cancel.performed += OnUICancelInput;
        }

        public override void Close()
        {
            _inputService.inputActions.UI.Cancel.performed -= OnUICancelInput;
            base.Close();
        }
        
        /// <summary>
        /// Sets up InventorySlotUI instances for each slot in the associated chest
        /// </summary>
        private void SetupUISlots()
        {
            // Adjust active ui slots amount to match linked slots count
            AdjustUISlotAmount(_linkedSlots.Count);
            
            BLog.Log("ChestUI",_uiSlots.Count);
            BLog.Log("ChestUI",_linkedSlots.Count);
            for (int i = 0; i < _linkedSlots.Count; ++i)
            {
                _uiSlots[i].LinkedSetup(_linkedSlots[i], true, true);
                _uiSlots[i].gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// De-activates additional InventoryUISlot instances, or instantiates new instances, to match the given
        /// desired count
        /// </summary>
        private void AdjustUISlotAmount(int count)
        {
            BLog.Log("ChestUI",$"Adjusting count (cur is {_uiSlots.Count} to {count}");
            int toAdd = count - _uiSlots.Count;
            for (int i = 0; i < toAdd; ++i)
            {
                BLog.Log("ChestUI", "Adding...");
                _uiSlots.Add(InstantiateUISlot());
            }
            for (int i = count; i < _uiSlots.Count; ++i)
            {
                BLog.Log("ChestUI","Removing...");
                _uiSlots[i].gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Instantiates a new InventoryUISlot instance under the intended parent transform
        /// </summary>
        /// <returns></returns>
        private InventorySlotCtrl InstantiateUISlot()
        {
            InventorySlotCtrl uiSlot =
                _diContainer.InstantiatePrefabForComponent<InventorySlotCtrl>(_inventorySlotPrefab);
            uiSlot.transform.SetParent(_slotParent);
            return uiSlot;
        }

        private void OnUICancelInput(InputAction.CallbackContext ctx)
        {
            Close();
        }
    }
}
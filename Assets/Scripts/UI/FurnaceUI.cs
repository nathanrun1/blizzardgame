using System;
using System.Collections.Generic;
using Blizzard.Inventory;
using Blizzard.Inventory.Crafting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Blizzard.UI
{
    public class FurnaceUI : UIBase
    {
        public struct Args
        {
            public InventorySlot ingredientSlot;
            public InventorySlot resultSlot;
            public InventorySlot fuelSlot;
        }

        [Header("References")]
        [SerializeField] CraftingDatabase _smeltingDatabase;
        // -- Smelting --
        [SerializeField] private InventorySlotCtrl _ingredientSlotUi;
        [SerializeField] private InventorySlotCtrl _resultSlotUi;
        [SerializeField] private InventorySlotCtrl _fuelSlotUi;

        [Inject] InventoryService _inventoryService;
        [Inject] UIService _uiService;

        private ItemAmountPair _storedIngredient;
        private ItemAmountPair _storedResult;
        private ItemAmountPair _storedFuel;

        public override void Setup(object args)
        {
            Args furnaceArgs;
            try
            {
                furnaceArgs = (Args)args;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("Incorrect argument type given!");
            }

            _ingredientSlotUi.LinkedSetup(furnaceArgs.ingredientSlot, true, true);
            _resultSlotUi.LinkedSetup(furnaceArgs.resultSlot, false, true);
            _fuelSlotUi.LinkedSetup(furnaceArgs.fuelSlot, true, true);
        }

        //private void OnCraft(CraftingRecipe recipe)
        //{
            
        //}
    }
}

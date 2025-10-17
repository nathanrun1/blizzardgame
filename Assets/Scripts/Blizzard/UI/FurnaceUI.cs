using System;
using UnityEngine;
using Zenject;
using Blizzard.Inventory;
using Blizzard.Inventory.Crafting;
using Blizzard.Obstacles.Concrete;
using Blizzard.UI.Core;
using Blizzard.UI.Inventory;

namespace Blizzard.UI
{
    public class FurnaceUI : UIBase
    {
        public struct Args
        {
            // Item slots from linked furnace
            public InventorySlot ingredientSlot;
            public InventorySlot resultSlot;
            public InventorySlot fuelSlot;

            /// <summary>
            /// State of linked furnace
            /// </summary>
            public Furnace.State furnaceState;
        }

        private Furnace.State _linkedFurnaceState = null;

        [Header("References")]
        [SerializeField] CraftingDatabase _smeltingDatabase;
        // -- Smelting --
        [SerializeField] private InventorySlotCtrl _ingredientSlotUi;
        [SerializeField] private InventorySlotCtrl _resultSlotUi;
        [SerializeField] private InventorySlotCtrl _fuelSlotUi;

        [Inject] InventoryService _inventoryService;
        [Inject] UIService _uiService;

        public override void Setup(object args)
        {
            Args furnaceArgs;
            try
            {
                furnaceArgs = (Args)args;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Incorrect argument type given!");
            }

            _ingredientSlotUi.LinkedSetup(furnaceArgs.ingredientSlot, true, true);
            _resultSlotUi.LinkedSetup(furnaceArgs.resultSlot, false, true);
            _fuelSlotUi.LinkedSetup(furnaceArgs.fuelSlot, true, true);
            _linkedFurnaceState = furnaceArgs.furnaceState;
        }

        //private void OnCraft(CraftingRecipe recipe)
        //{
            
        //}
    }
}

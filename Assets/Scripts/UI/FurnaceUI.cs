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
        [Header("References")]
        [SerializeField] CraftingDatabase _smeltingDatabase;

        // -- Smelting --
        [SerializeField] private InventorySlotCtrl _ingredientSlot;
        [SerializeField] private InventorySlotCtrl _resultSlot;
        [SerializeField] private InventorySlotCtrl _fuelSlot;

        [Inject] InventoryService _inventoryService;
        [Inject] UIService _uiService;

        private ItemAmountPair _storedIngredient;
        private ItemAmountPair _storedResult;
        private ItemAmountPair _storedFuel;

        public override void Setup(object args)
        {
            _ingredientSlot.Setup(null, 0);
            _resultSlot.Setup(null, 0);
            _fuelSlot.Setup(null, 0);
        }

        //private void OnCraft(CraftingRecipe recipe)
        //{
            
        //}
    }
}

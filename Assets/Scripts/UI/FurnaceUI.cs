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
        [SerializeField] InventorySlotCtrl _ingredientSlot;
        [SerializeField] InventorySlotCtrl _resultSlot;
        [SerializeField] InventorySlotCtrl _fuelSlot;

        [Inject] InventoryService _inventoryService;
        [Inject] UIService _uiService;

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

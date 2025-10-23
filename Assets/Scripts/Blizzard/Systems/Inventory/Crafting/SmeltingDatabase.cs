using Blizzard.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blizzard.Inventory.Crafting
{
    /// <summary>
    /// Database of available smelting recipes
    /// </summary>
    [CreateAssetMenu(fileName = "SmeltingDatabase", menuName = "ScriptableObjects/Crafting/SmeltingDatabase")]
    public class SmeltingDatabase : CraftingDatabase
    {
        /// <summary>
        /// Maps ingredient item ID to associated smelting crafting recipe
        /// </summary>
        [HideInInspector]
        public Dictionary<int, CraftingRecipe> SmeltRecipeMap
        {
            get
            {
                if (_smeltRecipeMap == null) InitSmeltRecipeMap();
                return _smeltRecipeMap;
            }
        }

        private Dictionary<int, CraftingRecipe> _smeltRecipeMap = null;

        private void InitSmeltRecipeMap()
        {
            _smeltRecipeMap = new Dictionary<int, CraftingRecipe>();
            foreach (var category in craftingCategories)
            foreach (var recipe in category.recipes)
            {
                Assert.IsTrue(recipe.cost.Count == 1, "Smelting recipe has more than one unique ingredient!");
                _smeltRecipeMap.Add(recipe.cost[0].item.id, recipe);
            }
        }
    }
}
using Blizzard.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.Inventory.Crafting
{
    /// <summary>
    /// A single crafting recipe
    /// </summary>
    [Serializable]
    public struct CraftingRecipe
    {
        /// <summary>
        /// Item that will be crafted
        /// </summary>
        public ItemData result;

        /// <summary>
        /// Amount of the resulting item that is crafted
        /// </summary>
        public int resultAmount;

        /// <summary>
        /// Items spent to craft the result
        /// </summary>
        public List<ItemAmountPair> cost;
    }

    /// <summary>
    /// A category of crafting recipes
    /// </summary>
    [Serializable]
    public struct CraftingCategory
    {
        public string categoryName;
        public CraftingRecipe[] recipes;
    }

    /// <summary>
    /// Database of available crafting recipes
    /// </summary>
    [CreateAssetMenu(fileName = "CraftingDatabase", menuName = "ScriptableObjects/Crafting/CraftingDatabase")]
    public class CraftingDatabase : ScriptableObject
    {
        public CraftingCategory[] craftingCategories;
    }
}
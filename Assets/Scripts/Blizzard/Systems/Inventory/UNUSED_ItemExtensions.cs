using UnityEngine;
using System;

//using System.Collections.Generic;
// >> CURRENTLY UNUSED! <<

//using System.Diagnostics;
//using ModestTree;

//namespace Blizzard.Inventory
//{
//    public static class ItemExtensions
//    {
//        /// <summary>
//        /// Gets an item's insulation value, intended only for clothing items.
//        /// </summary>
//        public static float GetInsulation(this Item clothingItem)
//        {
//            if (clothingItem.Category != ItemCategory.Clothing)
//            {
//                UnityEngine.BLog.LogWarning("Attempted to get insulation on a non-clothing item");
//                return default;
//            }

//            try
//            {
//                return (float)clothingItem.dynamicAttributes["insulation"];
//            }
//            catch (KeyNotFoundException _)
//            {
//                UnityEngine.BLog.LogError("Clothing item does not have 'insulation' set!");
//                return default;
//            }
//        }

//        /// <summary>
//        /// Sets an item's insulation to given value, intended only for clothing items.
//        /// </summary>
//        public static void SetInsulation(this Item clothingItem, float value)
//        {
//            Assert.That(0 <= value && value <= 1, "Insulation value must be between 0 and 1!");

//            if (clothingItem.Category != ItemCategory.Clothing)
//            {
//                UnityEngine.BLog.LogWarning("Attempted to set insulation on a non-clothing item");
//            }

//            try
//            {
//                clothingItem.dynamicAttributes["insulation"] = value;
//            }
//            catch (KeyNotFoundException _)
//            {
//                UnityEngine.BLog.LogError("Clothing item does not have 'insulation' set!");
//            }
//        }
//    }
//}
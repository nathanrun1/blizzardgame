using UnityEngine;
using System;
using System.Collections.Generic;

namespace Blizzard.Inventory
{
    /// <summary>
    /// An item and an associated amount
    /// </summary>
    [Serializable]
    public struct ItemAmountPair
    {
        public ItemData item;
        public int amount;
    }

    /// <summary>
    /// Configuration for a group of items
    /// </summary>
    [CreateAssetMenu(fileName = "ItemGroupData", menuName = "ScriptableObjects/Inventory/ItemGroupData")]
    public class ItemGroupData : ScriptableObject
    {
        public List<ItemAmountPair> items;
    }
}

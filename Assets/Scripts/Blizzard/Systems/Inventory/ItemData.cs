using System;
using UnityEngine;

namespace Blizzard.Inventory
{
    public enum ItemCategory
    {
        Tool,
        Resource,
        Clothing
    }

    /// <summary>
    /// Data passed to an item when it is equipped
    /// </summary>
    public struct EquipData
    {
        /// <summary>
        /// Index of its slot in the inventory
        /// </summary>
        public int slotIndex;
    }

    /// <summary>
    /// Configuration for a type of item
    /// </summary>
    public abstract class ItemData : ScriptableObject, IEquatable<ItemData>
    {
        /// <summary>
        /// Unique id
        /// </summary>
        public int id;
        /// <summary>
        /// Name of this item, as displayed in UI
        /// </summary>
        public string displayName;
        /// <summary>
        /// Icon of this item, as displayed in UI
        /// </summary>
        public Sprite icon;
        /// <summary>
        /// Max stack size
        /// </summary>
        public int stackSize;
        /// <summary>
        /// Category, determines how its used and its dynamic attributes
        /// </summary>
        [HideInInspector] public abstract ItemCategory category { get; set; }

        /// <summary>
        /// Color of ItemGainUI's "Count" text (the color of the number that shows you gained X amount of this item)
        /// </summary>
        [Header("Style")]
        [SerializeField]
        public Color32 itemGainColor = new Color32(255, 255, 255, 255);
        

        public bool Equals(ItemData other)
        {
            return !(other == null) && this.id == other.id;
        }

        /// <summary>
        /// Invoked when the player equips ("holds") this item from inventory
        /// </summary>
        public virtual void Equip(EquipData equipData) { }

        /// <summary>
        /// Invoked when the player unequips (stops "holding") this item type, assuming it is currently equipped
        /// </summary>
        public virtual void Unequip() { }
    }
}

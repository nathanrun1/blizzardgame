using UnityEngine;

namespace Blizzard.Inventory
{
    [CreateAssetMenu(fileName = "ClothingItemData", menuName = "ScriptableObjects/Inventory/ClothingItemData")]
    public class ClothingItemData : ItemData
    {
        /// <summary>
        /// Category of this item, determines how its used and its attributes
        /// </summary>
        [HideInInspector] public override ItemCategory category { get; set; } = ItemCategory.Clothing;
        /// <summary>
        /// Insulation provided to the player when worn
        /// </summary>
        public float insulation;
    }
}
using UnityEngine;

namespace Blizzard.Inventory
{
    [CreateAssetMenu(fileName = "ResourceItemData", menuName = "ScriptableObjects/Inventory/ResourceItemData")]
    public class ResourceItemData : ItemData
    {
        /// <summary>
        /// Category of this item, determines how its used and its attributes
        /// </summary>
        [HideInInspector]
        public override ItemCategory category { get; set; } = ItemCategory.Resource;
    }
}
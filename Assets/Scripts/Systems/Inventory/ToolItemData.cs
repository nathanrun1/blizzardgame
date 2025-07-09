using UnityEngine;
using Blizzard.Obstacles;
using Blizzard.Temperature;
using System;
using ModestTree;
using System.ComponentModel;
using System.Collections.Generic;

namespace Blizzard.Inventory
{
    [CreateAssetMenu(fileName = "ToolItemData", menuName = "ScriptableObjects/Inventory/ToolItemData")]
    public class ToolItemData : ItemData
    {
        /// <summary>
        /// Category of this item, determines how its used and its attributes
        /// </summary>
        [HideInInspector] public override ItemCategory category { get; set; } = ItemCategory.Tool;
    }
}
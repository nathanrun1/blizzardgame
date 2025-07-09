using Blizzard.Inventory;
using Blizzard.Temperature;
using ModestTree;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using Zenject;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// An obstacle that is harvestable by a tool, providing resources when harvested (destroyed)
    /// </summary>
    public class Harvestable : Obstacle
    {
        /// <summary>
        /// Health remaining, harvestable is destroyed (harvested) when health reaches 0
        /// </summary>
        public float Health { get; private set; }

        /// <summary>
        /// Type of tool that can harvest this harvestable, interpreted as bit field.
        /// </summary>
        public uint ToolType { get; private set; }


        /// <summary>
        /// Resources given to the player when harvested
        /// </summary>
        private ItemGroupData _resources;

        [Inject] private InventoryService _inventoryService;

        public void InitHarvestable(int startingHealth, ItemGroupData resources, uint toolType)
        {
            this.Health = startingHealth;
            this._resources = resources;
            this.ToolType = toolType;

            this.OnDestroy += OnHarvested;
        }

        [Button]
        public void Damage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                Destroy();
            }
        }

        private void OnHarvested()
        {
            List<ItemAmountPair> items = new(_resources.items);
            Debug.Log($"Harvested! Adding {items.Count} different items to inv!");
            Debug.Log(_inventoryService);
            _inventoryService.TryAddItems(items);

            if (!items.IsNullOrEmpty())
            {
                // Some items weren't succesfully added
                // TODO: Drop the items on the ground
            }
        }
    }
}

// "It is what it is."

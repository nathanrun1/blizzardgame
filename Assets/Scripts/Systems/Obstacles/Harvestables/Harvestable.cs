using Blizzard.Inventory;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections.Generic;
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

        [SerializeField] int _startingHealth = 100;
        [SerializeField] uint _toolType;
        /// <summary>
        /// Resources given to the player when harvested
        /// </summary>
        [SerializeField] private List<ItemAmountPair> _resources;

        [Inject] private InventoryService _inventoryService;

        private void Awake()
        {
            this.ToolType = _toolType;
        }

        public void Init()
        {
            this.Health = _startingHealth;
        }

        [Button]
        public virtual void Damage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                Harvest();
            }
        }

        /// <summary>
        /// "Harvests" the harvestable, destroying it and adding the resources to the player's inventory.
        /// </summary>
        private void Harvest()
        {
            List<ItemAmountPair> items = new(_resources);
            Debug.Log($"Harvested! Adding {items.Count} different items to inv!");
            Debug.Log(_inventoryService);
            _inventoryService.TryAddItems(items);

            if (!items.IsNullOrEmpty())
            {
                // Some items weren't succesfully added
                // TODO: Drop the items on the ground
            }

            Destroy();
        }
    }
}

// "It is what it is."

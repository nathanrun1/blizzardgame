using DG.Tweening;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Blizzard.Environment;
using Blizzard.Interfaces;
using Blizzard.Inventory;
using Blizzard.Player;
using Blizzard.Player.Tools;
using Blizzard.Utilities;
using Blizzard.UI.Core;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.Logging;

namespace Blizzard.Obstacles.Harvestables
{
    /// <summary>
    /// An obstacle that is harvestable by a tool, providing resources when harvested (destroyed)
    /// </summary>
    public class Harvestable : Damageable, IHittable
    {
        /// <summary>
        /// Type of tool(s) that can harvest this harvestable, interpreted as bit field.
        /// </summary>
        [Header("Harvestable properties")] 
        [SerializeField] public ToolType ToolTypes;

        /// <summary>
        /// Resources given to the player when harvested
        /// </summary>
        [SerializeField] private List<ItemAmountPair> _resources;

        [Inject] private InventoryService _inventoryService;
        [Inject] private EnvPrefabService _envPrefabService;
        [Inject] private PlayerService _playerService;
        [Inject] private UIService _uiService;

        public void Hit(int damage, ToolType toolType, out bool death)
        {
            // If hit by correct tool type, apply damage.
            death = false;
            if (toolType.HasFlag(ToolTypes))
                Damage(damage, DamageFlags.Player, _playerService.PlayerPosition, out death);
        }

        protected override void OnDeath(DamageFlags damageFlags, Vector3 sourcePosition)
        {
            if (damageFlags.HasFlag(DamageFlags.Player)) Harvest();
            else Destroy();
        }

        /// <summary>
        /// "Harvests" the harvestable, destroying it and adding the resources to the player's inventory.
        /// </summary>
        private void Harvest()
        {
            List<ItemAmountPair> items = new(_resources);

            _inventoryService.TryAddItemsWithAnim(_uiService, transform.position, items);

            if (!items.IsNullOrEmpty())
                // Some items weren't succesfully added (inventory full likely), drop instead
                foreach (var item in items)
                {
                    if (item.amount == 0) continue; // Sanity check
                    var dropObj = _envPrefabService.InstantiatePrefab("item_drop").GetComponent<ItemDrop>();
                    // Initialize drop at harvestable location with random offset
                    dropObj.transform.position = transform.position +
                                                 new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f), 0);
                    dropObj.Setup(item);
                }

            HarvestAnim(Destroy);
        }

        /// <summary>
        /// Feedback for the harvestable being fully harvested & destroyed
        /// </summary>
        private void HarvestAnim(System.Action onComplete = null)
        {
            // TODO: HarvestAnim FX
            onComplete?.Invoke();
        }
    }
}

// "It is what it is."
using DG.Tweening;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Blizzard.Environment;
using Blizzard.Inventory;
using Blizzard.Player;
using Blizzard.Player.Tools;
using Blizzard.Utilities;
using Blizzard.UI.Core;

namespace Blizzard.Obstacles.Harvestables
{
    /// <summary>
    /// An obstacle that is harvestable by a tool, providing resources when harvested (destroyed)
    /// </summary>
    public class Harvestable : Damageable
    {
        /// <summary>
        /// Type of tool(s) that can harvest this harvestable, interpreted as bit field.
        /// </summary>
        [Header("Harvestable properties")] [SerializeField]
        public ToolType ToolTypes;

        /// <summary>
        /// Resources given to the player when harvested
        /// </summary>
        [SerializeField] private List<ItemAmountPair> _resources;

        [Inject] private InventoryService _inventoryService;
        [Inject] private EnvPrefabService _envPrefabService;
        [Inject] private PlayerService _playerService;
        [Inject] private UIService _uiService;

        protected override void OnDamage(int damage, DamageFlags damageFlags, Vector3 sourcePosition)
        {
            if (Health > 0)
                StartCoroutine(FXAssistant.DamageAnim(transform, sourcePosition,
                    () => Health <= 0));
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
        /// Feedback for the harvestable being damaged by a tool
        /// </summary>
        private IEnumerator DamageAnim(Vector3 sourcePosition)
        {
            if (Health <= 0) yield break;
            var hitDirection = (transform.position - sourcePosition).normalized;
            var startPos = transform.position;
            var endPos = startPos + hitDirection * 0.05f;
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(endPos, 0.1f));
            sequence.Append(transform.DOMove(startPos, 0.1f));

            sequence.Play();
            yield return null;
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
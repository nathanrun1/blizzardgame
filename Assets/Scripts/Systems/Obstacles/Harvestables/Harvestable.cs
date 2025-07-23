using Blizzard.Environment;
using Blizzard.Inventory;
using Blizzard.Player;
using Blizzard.Utilities;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
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
        /// Type of tool(s) that can harvest this harvestable, interpreted as bit field.
        /// </summary>
        [SerializeField] public ToolType ToolTypes;


        [SerializeField] int _startingHealth = 100;
        /// <summary>
        /// Resources given to the player when harvested
        /// </summary>
        [SerializeField] private List<ItemAmountPair> _resources;

        [Inject] private InventoryService _inventoryService;
        [Inject] private EnvPrefabService _envPrefabService;
        [Inject] private PlayerService _playerService;

        public override void Init(float startingHeat, float startingInsulation)
        {
            this.Health = _startingHealth;
            base.Init(startingHeat, startingInsulation);
        }

        [Button]
        public virtual void Damage(int damage)
        {
            Health -= damage;
            Debug.Log("Taking " + damage + " damage, health is now " + Health);
            if (Health > 0) StartCoroutine(DamageAnim());
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
                // Some items weren't succesfully added (inventory full likely), drop instead
                foreach (ItemAmountPair item in items)
                {
                    if (item.amount == 0) continue; // Sanity check
                    ItemDrop dropObj = _envPrefabService.InstantiatePrefab("item_drop").GetComponent<ItemDrop>();
                    // Initialize drop at harvestable location with random offset
                    dropObj.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(-.5f, .5f), 0);
                    dropObj.Setup(item);
                }
            }

            HarvestAnim(() => Destroy());
        }

        /// <summary>
        /// Feedback for the harvestable being damaged by a tool
        /// </summary>
        /// <returns></returns>
        private IEnumerator DamageAnim()
        {
            Vector3 hitDirection = _playerService.GetFacingDirection();
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + hitDirection * 0.05f;
            Sequence sequence = DOTween.Sequence();
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
            // TODO: particle effects or sum
            onComplete?.Invoke();
        }
    }
}

// "It is what it is."

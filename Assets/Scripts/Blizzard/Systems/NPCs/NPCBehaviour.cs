using System;
using System.Collections.Generic;
using Blizzard.Constants;
using Blizzard.Environment;
using Blizzard.Interfaces;
using Blizzard.Inventory;
using Blizzard.Player;
using Blizzard.Player.Tools;
using Blizzard.UI.Core;
using Blizzard.Utilities;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Logging;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Blizzard.NPCs
{
    /// <summary>
    /// Base class for enemy behaviour implementation
    /// </summary>
    public abstract class NPCBehaviour : MonoBehaviour, IHittable, IStrikeable
    {
        [Inject] private InventoryService _inventoryService;
        [Inject] private UIService _uiService;
        [Inject] private EnvPrefabService _envPrefabService;
        
        [Header("EnemyBehaviour References")] 
        [SerializeField] private SpriteRenderer[] _spriteRenderers;
        [Header("EnemyBehaviour Config")]
        [SerializeField] private int _startingHealth;
        [SerializeField] private List<ItemAmountPair> _drops;

        private Color[] _spriteRendererInitialColors;
        private Sequence _damagedSequence;
        
        /// <summary>
        /// The enemy's health
        /// </summary>
        public int Health { get; protected set; }

        /// <summary>
        /// Invoked when enemy dies (health reaches 0)
        /// </summary>
        public event Action OnDeath;

        public virtual void Hit(int damage, ToolType toolType, out bool death)
        {
            TakeDamage(damage, out death, DamageFlags.Player);
        }
        
        public virtual void Strike(int damage, out bool death, DamageFlags damageFlags = DamageFlags.Player)
        {
            TakeDamage(damage, out death, damageFlags);
        }

        protected virtual void Awake()
        {
            // Store initial colors to reset visuals on enable
            _spriteRendererInitialColors = new Color[_spriteRenderers.Length];
            for (int i = 0; i < _spriteRenderers.Length; ++i)
            {
                _spriteRendererInitialColors[i] = _spriteRenderers[i].color;
            }
            // Setup damage animation
            _damagedSequence = FXAssistant.DOColorTint(_spriteRenderers, Color.red);
            _damagedSequence.SetLink(gameObject);
            _damagedSequence.SetAutoKill(false);
        }

        protected virtual void OnEnable()
        {
            // Reset health
            Health = _startingHealth;
            // Reset colors (animation may have been interrupted)... or maybe not tho TODO check
            for (int i = 0; i < _spriteRenderers.Length; ++i)
            {
                _spriteRenderers[i].color = _spriteRendererInitialColors[i];
            }
        }

        /// <summary>
        /// Take some amount of damage. If health reaches 0, causes death.
        /// </summary>
        protected virtual void TakeDamage(int damage, out bool death, DamageFlags damageFlags)
        {
            Health -= damage;

            death = Health <= 0;
            if (death)
            {
                Death(damageFlags);
                Health = 0;
            }
            else
            {
                // Damaged animation
                if (_damagedSequence.IsPlaying()) return;
                _damagedSequence.Restart();
                _damagedSequence.Play();
            }
        }

        /// <summary>
        /// Enemy death
        /// </summary>
        protected virtual void Death(DamageFlags damageFlags)
        {
            DropItems(directAdd: damageFlags.HasFlag(DamageFlags.Player));
            OnDeath?.Invoke();
        }
        
        /// <summary>
        /// Drops items specified by config, intended to be invoked on NPC death. If killed by player, will attempt
        /// to directly give the items.
        /// </summary>
        /// <param name="directAdd">Attempt to first add dropped items directly to player inventory</param>
        private void DropItems(bool directAdd)
        {
            List<ItemAmountPair> toDrop = new(_drops);
            if (directAdd) _inventoryService.TryAddItemsWithAnim(_uiService, transform.position, toDrop);
            foreach (ItemAmountPair pair in toDrop) // Drop remaining on the ground
            {
                ItemDrop dropObj = _envPrefabService.InstantiatePrefab("item_drop").GetComponent<ItemDrop>();
                dropObj.transform.position = (Vector2)transform.position +
                          RandomAssistant.RangeVector2(-GameConstants.CellSideLength / 2f, GameConstants.CellSideLength / 2f);
                dropObj.Setup(pair);
            }
        }
    }
}
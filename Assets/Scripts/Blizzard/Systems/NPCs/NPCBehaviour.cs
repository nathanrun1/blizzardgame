using System;
using Blizzard.Interfaces;
using Blizzard.Player.Tools;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.Logging;
using DG.Tweening;
using UnityEngine;

namespace Blizzard.NPCs
{
    /// <summary>
    /// Base class for enemy behaviour implementation
    /// </summary>
    public abstract class NPCBehaviour : MonoBehaviour, IHittable, IStrikeable
    {
        [Header("EnemyBehaviour References")] 
        [SerializeField] private SpriteRenderer[] _spriteRenderers;
        [Header("EnemyBehaviour Config")]
        [SerializeField] private int _startingHealth;

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
            TakeDamage(damage, out death);
        }
        
        public virtual void Strike(int damage, out bool death)
        {
            TakeDamage(damage, out death);
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
        protected virtual void TakeDamage(int damage, out bool death)
        {
            Health -= damage;

            death = Health <= 0;
            if (death)
            {
                Death();
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
        protected virtual void Death()
        {
            OnDeath?.Invoke();
        }
    }
}
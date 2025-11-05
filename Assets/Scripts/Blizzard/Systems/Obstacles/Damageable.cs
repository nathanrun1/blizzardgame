using System;
using System.Diagnostics;
using Blizzard.Utilities.Assistants;
using Sirenix.OdinInspector;
using UnityEngine;
using Blizzard.Utilities.Logging;
using DG.Tweening;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Describes the source of damage
    /// </summary>
    [Flags]
    public enum DamageFlags
    {
        Player = 1 << 0,
        Enemy = 1 << 1
    }

    /// <summary>
    /// An obstacle that has health
    /// </summary>
    public class Damageable : Obstacle
    {
        /// <summary>
        /// Health remaining, harvestable is destroyed (harvested) when health reaches 0
        /// </summary>
        public float Health { get; private set; }

        [Button]
        [Conditional("UNITY_EDITOR")]
        public void PrintHealth()
        {
            BLog.Log(Health);
        }

        [Header("Damageable References")]
        [SerializeField] private SpriteRenderer[] _spriteRenderers;
        [Header("Damageable Properties")] 
        [SerializeField] private int _startingHealth = 100;

        /// <summary>
        /// Sequence for the damaged animation
        /// </summary>
        private Sequence _damagedSequence;

        public override void Init(ObstacleData obstacleData)
        {
            Health = _startingHealth;
            base.Init(obstacleData);
        }

        /// <summary>
        /// Inflicts damage
        /// </summary>
        /// <param name="damage">Amount of damage to inflict</param>
        /// <param name="damageFlags">Damage flags associated with damage source</param>
        /// <param name="sourcePosition">World position of the damage source</param>
        /// <param name="death">Set to true if health less than or equal to 0 after damage, false otherwise</param>
        [Button]
        public void Damage(int damage, DamageFlags damageFlags, Vector3 sourcePosition, out bool death)
        {
            if (Health <= 0)
            {
                // Already dead
                death = true;
                return;
            }
            
            Health -= damage;

            BLog.Log("Taking " + damage + " damage, health is now " + Health);
            OnDamage(damage, damageFlags, sourcePosition);

            if (Health <= 0)
            {
                Health = 0;
                death = true;
                OnDeath(damageFlags, sourcePosition);
            }
            else
            {
                death = false;
            }
        }

        /// <summary>
        /// Invoked when damaged
        /// </summary>
        /// <param name="damage">Damage inflicted</param>
        /// <param name="damageFlags">Damage flags associated with damage source</param>
        /// <param name="sourcePosition">World position of the damage source</param>s
        protected virtual void OnDamage(int damage, DamageFlags damageFlags, Vector3 sourcePosition)
        {
            if (Health > 0 && (_damagedSequence == null || !_damagedSequence.IsPlaying()))
            {
                // Damage animation
                _damagedSequence = DOTween.Sequence();
                if (damageFlags.HasFlag(DamageFlags.Enemy))
                    _damagedSequence.Join(FXAssistant.DOColorTint(_spriteRenderers, Color.red));
                //StartCoroutine(FXAssistant.TintSequenceCoroutine(_spriteRenderers, Color.red));
                _damagedSequence.Join(FXAssistant.DODamageBounce(transform, sourcePosition));
                _damagedSequence.SetLink(gameObject);
                _damagedSequence.Play();
            }
        }

        /// <summary>
        /// Invoked when health reaches 0 as a result of being damaged
        /// </summary>
        /// <param name="damageFlags">Damage flags associated with damage source</param>
        /// <param name="sourcePosition">World position of the damage source</param>
        protected virtual void OnDeath(DamageFlags damageFlags, Vector3 sourcePosition)
        {
        }
    }
}

// "It is what it is."
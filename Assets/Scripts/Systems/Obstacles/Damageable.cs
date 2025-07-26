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
    /// An obstacle that has health
    /// </summary>
    public class Damageable : Obstacle
    {
        /// <summary>
        /// Health remaining, harvestable is destroyed (harvested) when health reaches 0
        /// </summary>
        public float Health { get; private set; }

        [Header("Damageable Properties")]
        [SerializeField] int _startingHealth = 100;

        public override void Init(ObstacleData obstacleData)
        {
            this.Health = _startingHealth;
            base.Init(obstacleData);
        }
        
        /// <summary>
        /// Inflicts damage
        /// </summary>
        /// <param name="death">Set to true if health <= 0 after damage, false otherwise</param>
        [Button]
        public void Damage(int damage, out bool death)
        {
            Health -= damage;

            Debug.Log("Taking " + damage + " damage, health is now " + Health);
            OnDamage(damage);

            if (Health <= 0)
            {
                Health = 0;
                death = true;
                OnDeath();
            }
            else death = false;
        }

        /// <summary>
        /// Invoked when damaged
        /// </summary>
        /// <param name="damage">Damage inflicted</param>
        protected virtual void OnDamage(int damage) { }
        /// <summary>
        /// Invoked when health reaches 0
        /// </summary>
        protected virtual void OnDeath() { }
    }
}

// "It is what it is."

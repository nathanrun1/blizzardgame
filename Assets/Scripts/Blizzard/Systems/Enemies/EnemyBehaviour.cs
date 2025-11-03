using System;
using Blizzard.Interfaces;
using Blizzard.Player.Tools;
using Blizzard.Utilities;
using UnityEngine;

namespace Blizzard.Enemies
{
    /// <summary>
    /// Base class for enemy behaviour implementation
    /// </summary>
    public abstract class EnemyBehaviour : MonoBehaviour, IHittable, IStrikeable
    {
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

        /// <summary>
        /// Take some amount of damage. If health goes below 0, causes death.
        /// </summary>
        protected virtual void TakeDamage(int damage, out bool death)
        {
            Health -= damage;
            
            death = Health < 0;
            if (death)
            {
                Death();
                Health = 0;
            }
        }

        /// <summary>
        /// Enemy death
        /// </summary>
        protected virtual void Death()
        {
            gameObject.SetActive(false); // TODO
            OnDeath?.Invoke();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Blizzard.NPCs;
using Blizzard.NPCs.Core;
using Blizzard.Player;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Logging;

namespace Blizzard.Obstacles.Concrete
{
    public class Crossbow : Structure
    {
        private static readonly int StandbyAnimProperty = Animator.StringToHash("Standby");
        
        [Inject] private NPCService _npcService;
        [Inject] private PlayerService _playerService;
        
        /// <summary>
        /// Crossbow's body
        /// </summary>
        [Header("References")]
        [SerializeField] private GameObject _crossbowBody;
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationClip _fireAnimationClip;
        /// <summary>
        /// Max distance of crossbow target
        /// </summary>
        [Header("Config")]
        [SerializeField] private float _maxRange;
        /// <summary>
        /// Projectile firing cooldown
        /// </summary>
        [SerializeField] private float _fireCooldown;
        /// <summary>
        /// Damage to inflict
        /// </summary>
        [SerializeField] private int _damage = 10;

        private NPCBehaviour _currentTarget;

        private void Awake()
        {
            ConfigureAnimations();
        }

        private void FixedUpdate()
        {
            if (TryGetClosestEnemy(out NPCBehaviour closestEnemy))
            {
                _currentTarget = closestEnemy;
                PointBodyAt(_currentTarget.transform.position);
            }
            else
            {
                _currentTarget = null;
            }
            
            _animator.SetBool(StandbyAnimProperty, !_currentTarget);
        }
        
        /// <summary>
        /// Adjusts animations to align with config
        /// </summary>
        private void ConfigureAnimations()
        {
            // Adjust animation speed based on cooldown
            _animator.speed = _fireAnimationClip.length / _fireCooldown;
        }

        /// <summary>
        /// Gets the closest enemy to the crossbow within range, if any.
        /// </summary>
        /// <returns>Closest enemy or null if none in range</returns>
        private bool TryGetClosestEnemy(out NPCBehaviour closestNpc)
        {
            List<NPCBehaviour> nearest = _npcService.Quadtrees[NPCID.Zombie].GetKNearestNPCs(transform.position, 1, _maxRange);
            closestNpc = nearest.Count > 0 ? nearest[0] : null;
            return nearest.Count > 0;
        }

        private void PointBodyAt(Vector2 position)
        {
            Vector2 diff = position - (Vector2)transform.position;
            _crossbowBody.transform.localEulerAngles = new Vector3(
                _crossbowBody.transform.rotation.x, 
                _crossbowBody.transform.rotation.y,
                Mathf.Rad2Deg * Mathf.Atan2(diff.y, diff.x) - 90f);
        }

        private void OnCrossbowFire()
        {
            _currentTarget?.Strike(_damage, out _, DamageFlags.Structure);
        }
    }
}

// "I really just be eating oats with the lid"

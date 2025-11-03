using System;
using System.Collections.Generic;
using Blizzard.Enemies;
using Blizzard.Enemies.Core;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using Blizzard.Inventory;
using Blizzard.UI;
using Blizzard.UI.Core;
using Blizzard.Inventory.Crafting;
using Blizzard.Player;
using Blizzard.Utilities.Logging;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Blizzard.Obstacles.Concrete
{
    public class Crossbow : Structure
    {
        [Inject] private EnemyService _enemyService;
        
        /// <summary>
        /// Crossbow's body
        /// </summary>
        [Header("References")]
        [SerializeField] private GameObject _crossbowBody;
        /// <summary>
        /// Max distance of crossbow target
        /// </summary>
        [Header("Config")]
        [SerializeField] private float _maxRange;

        private void Update()
        {
            if (TryGetClosestEnemy(out EnemyBehaviour closestEnemy))
            {
                PointBodyAt(closestEnemy.transform.position);
            }
        }

        /// <summary>
        /// Gets the closest enemy to the crossbow within range, if any.
        /// </summary>
        /// <returns>Closest enemy or null if none in range</returns>
        private bool TryGetClosestEnemy(out EnemyBehaviour closestEnemy)
        {
            List<EnemyBehaviour> nearest = _enemyService.Quadtree.GetKNearestEnemies(transform.position, 1, _maxRange);
            closestEnemy = nearest.Count > 0 ? nearest[0] : null;
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
    }
}

// "I really just be eating oats with the lid"

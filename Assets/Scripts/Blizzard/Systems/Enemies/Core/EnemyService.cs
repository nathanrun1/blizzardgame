using System.Collections;
using System.Collections.Generic;
using Blizzard.Constants;
using Blizzard.Enemies.Core;
using Blizzard.Utilities.Logging;
using UnityEngine;
using Zenject;

namespace Blizzard.Enemies.Core
{
    public class EnemyService : IFixedTickable
    {
        [Inject] private DiContainer _diContainer;

        /// <summary>
        /// Quadtree containing locations of all enemies. Can be used for enemy position queries.
        /// </summary>
        public readonly EnemyQuadtree Quadtree = new();
        private IEnumerator _quadtreeTick;
        
        /// <summary>
        /// Maps enemy id (int) to enemy data
        /// </summary>
        private readonly Dictionary<int, EnemyData> _enemyDict = new();
        /// <summary>
        /// Parent transform to spawned enemies
        /// </summary>
        private readonly Transform _enemyParent;

        /// <summary>
        /// Maps enemy id (int) to stack containing inactive instances. Prioritizes inactive
        /// instances of an enemy for spawning before instantiating.
        /// </summary>
        private readonly Dictionary<int, Stack<EnemyBehaviour>> _inactiveEnemies = new();

        /// <summary>
        /// Initializes EnemyService
        /// </summary>
        /// <param name="enemyParent">Parent to spawned enemies</param>
        public EnemyService(Transform enemyParent)
        {
            _enemyParent = enemyParent;
        }

        /// <summary>
        /// Spawns enemy at given position
        /// </summary>
        /// <returns>Spawned enemy instance</returns>
        public EnemyBehaviour SpawnEnemy(EnemyID enemyID, Vector3 spawnPosition)
        {
            return SpawnEnemy(_enemyDict[(int)enemyID], spawnPosition);
        }
        
        
        private EnemyBehaviour SpawnEnemy(EnemyData enemyData, Vector3 spawnPosition)
        {
            // Pull from pool if pool non-empty, else instantiate directly
            EnemyBehaviour enemyInstance =_inactiveEnemies[enemyData.ID].Count > 0 ? 
                _inactiveEnemies[enemyData.ID].Pop() :
                _diContainer.InstantiatePrefabForComponent<EnemyBehaviour>(enemyData.enemyPrefab);
            enemyInstance.transform.position = spawnPosition;
            enemyInstance.transform.SetParent(_enemyParent);
            Quadtree.Add(enemyInstance);
            int enemyID = enemyData.ID;
            enemyInstance.OnDeath += () =>
            {
                // On enemy death: Remove from quadtree and transfer to inactive pool
                Quadtree.Remove(enemyInstance); 
                _inactiveEnemies[enemyID].Push(enemyInstance);
            };
            enemyInstance.gameObject.SetActive(true);
            return enemyInstance;
        }
        
        [Inject]
        private void Initialize(EnemyDatabase enemyDatabase)
        {
            BLog.Log("Initializing UI prefab dictionaries");
            foreach (EnemyData enemyData in enemyDatabase.enemyDatas)
            {
                _enemyDict.Add(enemyData.ID, enemyData);
                InitPool(enemyData);
            }

            _quadtreeTick = Quadtree.Tick();
        }

        /// <summary>
        /// Initializes starting inactive pool for given enemy type
        /// </summary>
        /// <param name="enemyData"></param>
        private void InitPool(EnemyData enemyData)
        {
            _inactiveEnemies.Add(enemyData.ID, new Stack<EnemyBehaviour>());
            enemyData.enemyPrefab.gameObject.SetActive(false);
            
            // Instantiate inactive enemies to fill pool
            for (int i = 0; i < EnemyConstants.StartInactivePoolSize; ++i)
            {
                _inactiveEnemies[enemyData.ID].Push(
                    _diContainer.InstantiatePrefabForComponent<EnemyBehaviour>(enemyData.enemyPrefab));
            }
        }

        public void FixedTick()
        {
            // Tick the quadtree
            _quadtreeTick.MoveNext();
        }
    }
}
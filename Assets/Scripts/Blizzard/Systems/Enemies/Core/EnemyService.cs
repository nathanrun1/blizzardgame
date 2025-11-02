using System.Collections.Generic;
using Blizzard.Constants;
using Blizzard.Enemies.Core;
using Blizzard.Utilities.Logging;
using UnityEngine;
using Zenject;

namespace Blizzard.Enemies.Core
{
    public class EnemyService
    {
        [Inject] private DiContainer _diContainer;
        
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
        private readonly Dictionary<int, Stack<GameObject>> _inactiveEnemies;

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
        public GameObject SpawnEnemy(EnemyData enemyData, Vector3 spawnPosition)
        {
            // Pull from pool if pool non-empty, else instantiate directly
            var enemyInstance =_inactiveEnemies[enemyData.id].Count > 0 ? 
                _inactiveEnemies[enemyData.id].Pop() :
                _diContainer.InstantiatePrefab(enemyData.enemyPrefab);
            enemyInstance.transform.position = spawnPosition;
            // TODO: Reset enemy state when spawning
            return enemyInstance;
        }
        
        [Inject]
        private void Initialize(EnemyDatabase enemyDatabase)
        {
            BLog.Log("Initializing UI prefab dictionaries");
            foreach (EnemyData enemyData in enemyDatabase.enemyDatas)
            {
                _enemyDict.Add(enemyData.id, enemyData);
                InitPool(enemyData);
            }
        }

        /// <summary>
        /// Initializes starting inactive pool for given enemy type
        /// </summary>
        /// <param name="enemyData"></param>
        private void InitPool(EnemyData enemyData)
        {
            _inactiveEnemies.Add(enemyData.id, new Stack<GameObject>());
            enemyData.enemyPrefab.SetActive(false);
            
            // Instantiate inactive enemies to fill pool
            for (int i = 0; i < EnemyConstants.StartInactivePoolSize; ++i)
            {
                _inactiveEnemies[enemyData.id].Push(_diContainer.InstantiatePrefab(enemyData.enemyPrefab));
            }
        }
    }
}
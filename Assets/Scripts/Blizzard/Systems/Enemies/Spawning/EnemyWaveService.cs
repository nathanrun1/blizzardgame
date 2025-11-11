using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Enemies.Core;
using Blizzard.Obstacles;
using UnityEngine;
using Zenject;

namespace Blizzard.Enemies.Spawning
{
    /// <summary>
    /// A wave of enemies (i.e. a collection of enemy groups spawned at intervals)
    /// </summary>
    [Serializable]
    public struct EnemyWave
    {
        /// <summary>
        /// Groups of enemies to spawn
        /// </summary>
        public EnemyGroup[] enemyGroups;
    }

    [Serializable]
    public struct EnemyGroup
    {
        /// <summary>
        /// Type of enemy to spawn
        /// </summary>
        public EnemyID enemyId;
        /// <summary>
        /// Amount of enemies per group
        /// </summary>
        public int groupSize;
        /// <summary>
        /// Amount of time to wait before spawning this group
        /// </summary>
        public float spawnDelay;
    }
    
    /// <summary>
    /// Service responsible for spawning enemy waves
    /// </summary>
    public class EnemyWaveService : IInitializable, IFixedTickable
    {
        [Inject] private DiContainer _diContainer;
        [Inject] private EnemyService _enemyService;
        [Inject] private ObstacleGridService _obstacleGridService;
    
        /// <summary>
        /// Range of valid locations to spawn enemies
        /// </summary>
        private EnemySpawnRange _enemySpawnRange;
        private IEnumerator _enemySpawnRangeTick;

        private readonly SortedDictionary<float, EnemyGroup> _enemyGroupsToSpawn = new();

        public void Initialize()
        {
            _enemySpawnRange = _diContainer.Instantiate<EnemySpawnRange>();
            _enemySpawnRangeTick = _enemySpawnRange.Tick();
        }

        public void FixedTick()
        {
            // Keep enemy spawn range updated
            _enemySpawnRangeTick.MoveNext();

            float currentTime = Time.realtimeSinceStartup;
            if (_enemyGroupsToSpawn.Count == 0 || _enemyGroupsToSpawn.Keys.First() >= currentTime) return;
            // Spawn first group that is ready to spawn (if multiple, will spawn on future frames)
            SpawnEnemyGroup(_enemyGroupsToSpawn.Values.First());
            _enemyGroupsToSpawn.Remove(_enemyGroupsToSpawn.Keys.First());
        }

        /// <summary>
        /// Starts the given enemy wave after a (optional) given delay
        /// </summary>
        public void StartEnemyWave(EnemyWave enemyWave, float delay = 0f)
        {
            float currentTime = Time.realtimeSinceStartup;
            float cumulativeDelay = delay;
            foreach (EnemyGroup enemyGroup in enemyWave.enemyGroups)
            {
                // Spawn group after delays of all previous groups + wave delay
                cumulativeDelay += enemyGroup.spawnDelay;
                _enemyGroupsToSpawn.Add(currentTime + cumulativeDelay, enemyGroup);
            }
        }


        private void SpawnEnemyGroup(EnemyGroup enemyGroup)
        {
            for (int i = 0; i < enemyGroup.groupSize; ++i)
            {
                _enemyService.SpawnEnemy(
                    enemyGroup.enemyId,
                    _enemySpawnRange.GetRandomEnemySpawnLocation()
                );
            }
        }
    }
}
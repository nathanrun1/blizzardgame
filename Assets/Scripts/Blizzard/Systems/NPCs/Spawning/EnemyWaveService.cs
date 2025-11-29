using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Constants;
using Blizzard.NPCs.Core;
using Blizzard.Obstacles;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Blizzard.NPCs.Spawning
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
    public class EnemyGroup
    {
        /// <summary>
        /// Type of enemy to spawn
        /// </summary>
        [FormerlySerializedAs("enemyId")] public NPCID npcid;
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
        [Inject] private NPCService _npcService;
        [Inject] private ObstacleGridService _obstacleGridService;
    
        /// <summary>
        /// Range of valid locations to spawn enemies
        /// </summary>
        private EnemySpawnRange _enemySpawnRange;
        private IEnumerator _enemySpawnRangeTick;

        /// <summary>
        /// Enemy groups set to start spawning at a given time
        /// </summary>
        private readonly SortedDictionary<float, EnemyGroup> _readyEnemyGroups = new();

        public void Initialize()
        {
            _enemySpawnRange = _diContainer.Instantiate<EnemySpawnRange>();
            _enemySpawnRangeTick = _enemySpawnRange.Tick();
        }

        public void FixedTick()
        {
            // Keep enemy spawn range updated
            _enemySpawnRangeTick.MoveNext();
            SpawnNextEnemy();
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
                _readyEnemyGroups.Add(currentTime + cumulativeDelay, enemyGroup);
            }
        }

        /// <summary>
        /// Spawns next batch of enemies ready to spawn
        /// </summary>
        private void SpawnNextEnemy()
        {
            if (_readyEnemyGroups.Count == 0 || _readyEnemyGroups.Keys.First() >= Time.realtimeSinceStartup) return;
            for (int i = 0; i < EnemyConstants.MaxEnemySpawnsPerTick; ++i)
            {
                _npcService.SpawnEnemy(
                    _readyEnemyGroups.Values.First().npcid,
                    _enemySpawnRange.GetRandomEnemySpawnLocation()
                );
                _readyEnemyGroups.Values.First().groupSize -= 1;  // Decrement group size
                if (_readyEnemyGroups.Values.First().groupSize != 0) continue;
                
                _readyEnemyGroups.Remove(_readyEnemyGroups.Keys.First());
                if (_readyEnemyGroups.Count == 0 || _readyEnemyGroups.Keys.First() >= Time.realtimeSinceStartup) return;
            }
        }
    }
}
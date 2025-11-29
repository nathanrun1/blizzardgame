using System.Collections;
using System.Collections.Generic;
using Blizzard.Constants;
using Blizzard.Grid;
using Blizzard.Obstacles;
using Blizzard.Utilities.Logging;
using UnityEngine;
using Zenject;

namespace Blizzard.NPCs.Core
{
    /// <summary>
    /// Service responsible for managing and spawning NPCs
    /// </summary>
    public class NPCService : IFixedTickable
    {
        [Inject] private DiContainer _diContainer;
        [Inject] private ObstacleGridService _obstacleGridService;

        /// <summary>
        /// Quadtree containing locations of all NPCs. Can be used for enemy position queries.
        /// </summary>
        public readonly NPCQuadtree Quadtree = new();
        
        private IEnumerator _quadtreeTick;
        /// <summary>
        /// Maps enemy id (int) to enemy data
        /// </summary>
        private readonly Dictionary<int, NPCData> _enemyDict = new();
        /// <summary>
        /// Parent transform to spawned enemies
        /// </summary>
        private readonly Transform _enemyParent;
        /// <summary>
        /// Maps enemy id (int) to stack containing inactive instances. Prioritizes inactive
        /// instances of an enemy for spawning before instantiating.
        /// </summary>
        private readonly Dictionary<int, Stack<NPCBehaviour>> _inactiveEnemies = new();

        /// <summary>
        /// Initializes EnemyService
        /// </summary>
        /// <param name="enemyParent">Parent to spawned enemies</param>
        public NPCService(Transform enemyParent)
        {
            _enemyParent = enemyParent;
        }

        /// <summary>
        /// Spawns enemy at given position
        /// </summary>
        /// <returns>Spawned enemy instance</returns>
        public NPCBehaviour SpawnEnemy(NPCID npcid, Vector3 spawnPosition)
        {
            return SpawnEnemy(_enemyDict[(int)npcid], spawnPosition);
        }

        /// <summary>
        /// Spawns enemy at given grid position
        /// </summary>
        /// <returns>Spawned enemy instance</returns>
        public NPCBehaviour SpawnEnemy(NPCID npcid, Vector2Int spawnPosition)
        {
            Vector3 spawnPositionWorld = _obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                .CellToWorldPosCenter(spawnPosition);
            return SpawnEnemy(_enemyDict[(int)npcid], spawnPositionWorld);
        }
        
        
        private NPCBehaviour SpawnEnemy(NPCData npcData, Vector3 spawnPosition)
        {
            // Pull from pool if pool non-empty, else instantiate directly
            NPCBehaviour npcInstance =_inactiveEnemies[npcData.ID].Count > 0 ? 
                _inactiveEnemies[npcData.ID].Pop() :
                _diContainer.InstantiatePrefabForComponent<NPCBehaviour>(npcData.npcPrefab);
            
            npcInstance.transform.position = spawnPosition;
            npcInstance.transform.SetParent(_enemyParent);
            Quadtree.Add(npcInstance);
            
            int enemyID = npcData.ID;
            npcInstance.OnDeath += () =>
            {
                // On enemy death: Remove from quadtree and transfer to inactive pool
                if (!npcInstance.gameObject.activeInHierarchy) return; // Do nothing if already inactive
                npcInstance.gameObject.SetActive(false);
                Quadtree.Remove(npcInstance); 
                _inactiveEnemies[enemyID].Push(npcInstance);
            };
            npcInstance.gameObject.SetActive(true);
            return npcInstance;
        }
        
        [Inject]
        private void Initialize(NPCDatabase npcDatabase)
        {
            BLog.Log("Initializing UI prefab dictionaries");
            foreach (NPCData enemyData in npcDatabase.npcDatas)
            {
                _enemyDict.Add(enemyData.ID, enemyData);
                InitPool(enemyData);
            }

            _quadtreeTick = Quadtree.Tick();
        }

        /// <summary>
        /// Initializes starting inactive pool for given enemy type
        /// </summary>
        /// <param name="npcData"></param>
        private void InitPool(NPCData npcData)
        {
            _inactiveEnemies.Add(npcData.ID, new Stack<NPCBehaviour>());
            npcData.npcPrefab.gameObject.SetActive(false);
            
            // Instantiate inactive enemies to fill pool
            for (int i = 0; i < EnemyConstants.StartInactivePoolSize; ++i)
            {
                _inactiveEnemies[npcData.ID].Push(
                    _diContainer.InstantiatePrefabForComponent<NPCBehaviour>(npcData.npcPrefab));
            }
        }

        public void FixedTick()
        {
            // Tick the quadtree
            _quadtreeTick.MoveNext();
        }
    }
}
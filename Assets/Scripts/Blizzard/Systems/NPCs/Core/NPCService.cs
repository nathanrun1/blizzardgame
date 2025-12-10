using System;
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
        /// Quadtrees containing locations of all NPCs of different types. Useful for NPC position querying.
        /// </summary>
        public readonly Dictionary<NPCID, NPCQuadtree> Quadtrees = new();
        
        private List<IEnumerator> _quadtreeTicks = new();
        /// <summary>
        /// Maps enemy id (int) to enemy data
        /// </summary>
        private readonly Dictionary<int, NPCData> _enemyDict = new();
        /// <summary>
        /// Parent transform to spawned enemies
        /// </summary>
        private readonly Transform _npcParent;
        /// <summary>
        /// Maps enemy id (int) to stack containing inactive instances. Prioritizes inactive
        /// instances of an enemy for spawning before instantiating.
        /// </summary>
        private readonly Dictionary<int, Stack<NPCBehaviour>> _inactiveNPCs = new();

        /// <summary>
        /// Initializes EnemyService
        /// </summary>
        /// <param name="npcParent">Parent to spawned enemies</param>
        public NPCService(Transform npcParent)
        {
            _npcParent = npcParent;
            foreach (NPCID npcId in Enum.GetValues(typeof(NPCID)))
            {
                Quadtrees.Add(npcId, new NPCQuadtree());
                _quadtreeTicks.Add(Quadtrees[npcId].Tick());
            }
        }

        /// <summary>
        /// Spawns enemy at given position
        /// </summary>
        /// <returns>Spawned enemy instance</returns>
        public NPCBehaviour SpawnNPC(NPCID npcid, Vector3 spawnPosition)
        {
            return SpawnNPC(_enemyDict[(int)npcid], spawnPosition);
        }

        /// <summary>
        /// Spawns enemy at given grid position
        /// </summary>
        /// <returns>Spawned enemy instance</returns>
        public NPCBehaviour SpawnNPC(NPCID npcid, Vector2Int spawnPosition)
        {
            Vector3 spawnPositionWorld = _obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                .CellToWorldPosCenter(spawnPosition);
            return SpawnNPC(_enemyDict[(int)npcid], spawnPositionWorld);
        }
        
        
        private NPCBehaviour SpawnNPC(NPCData npcData, Vector3 spawnPosition)
        {
            // Pull from pool if pool non-empty, else instantiate directly
            NPCBehaviour npcInstance =_inactiveNPCs[npcData.ID].Count > 0 ? 
                _inactiveNPCs[npcData.ID].Pop() :
                _diContainer.InstantiatePrefabForComponent<NPCBehaviour>(npcData.npcPrefab);
            
            npcInstance.transform.position = spawnPosition;
            npcInstance.transform.SetParent(_npcParent);
            Quadtrees[(NPCID)npcData.ID].Add(npcInstance);
            
            int enemyID = npcData.ID;
            npcInstance.OnDeath += () =>
            {
                // On enemy death: Remove from quadtree and transfer to inactive pool
                if (!npcInstance.gameObject.activeInHierarchy) return; // Do nothing if already inactive
                npcInstance.gameObject.SetActive(false);
                Quadtrees[(NPCID)npcData.ID].Remove(npcInstance); 
                _inactiveNPCs[enemyID].Push(npcInstance);
            };
            npcInstance.gameObject.SetActive(true);
            return npcInstance;
        }
        
        [Inject]
        private void Initialize(NPCDatabase npcDatabase)
        {
            BLog.Log("Initializing NPC prefab dictionary");
            foreach (NPCData npcData in npcDatabase.npcDatas)
            {
                _enemyDict.Add(npcData.ID, npcData);
                InitPool(npcData);
            }
        }

        /// <summary>
        /// Initializes starting inactive pool for given enemy type
        /// </summary>
        /// <param name="npcData"></param>
        private void InitPool(NPCData npcData)
        {
            _inactiveNPCs.Add(npcData.ID, new Stack<NPCBehaviour>());
            npcData.npcPrefab.gameObject.SetActive(false);
            
            // Instantiate inactive enemies to fill pool
            for (int i = 0; i < EnemyConstants.StartInactivePoolSize; ++i)
            {
                _inactiveNPCs[npcData.ID].Push(
                    _diContainer.InstantiatePrefabForComponent<NPCBehaviour>(npcData.npcPrefab));
            }
        }

        public void FixedTick()
        {
            // Tick each quadtree
            foreach (IEnumerator qtTick in _quadtreeTicks) 
                qtTick.MoveNext();
        }
    }
}
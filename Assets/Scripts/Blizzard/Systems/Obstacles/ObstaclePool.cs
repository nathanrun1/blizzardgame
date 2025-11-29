using System.Collections.Generic;
using Blizzard.Constants;
using Unity.Assertions;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Manages the creation/destruction of obstacles using pooling
    /// </summary>
    public class ObstaclePool
    {
        private readonly DiContainer _diContainer;
        
        private readonly Dictionary<ObstacleData, ObjectPool<Obstacle>> _obstacleTypePools;
        
        public ObstaclePool(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        /// <summary>
        /// Initializes an obstacle pool for the given obstacle type. Afterward, will instead use pooling
        /// for this obstacle type rather than directly instantiating/destroying.
        /// </summary>
        public void InitPool(ObstacleData obstacleData)
        {
            if (_obstacleTypePools.ContainsKey(obstacleData)) return;
            _obstacleTypePools.Add(obstacleData, new ObjectPool<Obstacle>(
                createFunc: () => InstantiateObstacle(obstacleData),
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroyObstacle,
                defaultCapacity: ObstacleConstants.ChunkSize,
                maxSize: ObstacleConstants.ActiveChunkRangeSize
            ));
        }

        /// <summary>
        /// Retrieves a new instance of the given obstacle.
        /// </summary>
        /// <param name="obstacleData"></param>
        /// <returns></returns>
        public Obstacle Get(ObstacleData obstacleData)
        {
            return _obstacleTypePools.TryGetValue(obstacleData, out ObjectPool<Obstacle> pool) ? pool.Get() : InstantiateObstacle(obstacleData);
        }

        public void Release(Obstacle obstacle)
        {
            if (_obstacleTypePools.TryGetValue(obstacle.ObstacleData, out ObjectPool<Obstacle> pool))
                pool.Release(obstacle);
            else
                OnDestroyObstacle(obstacle);
        }
        
        private Obstacle InstantiateObstacle(ObstacleData obstacleData)
        {
            Assert.IsTrue(obstacleData.obstaclePrefab, "obstaclePrefab not set!");
            Obstacle obstacle = _diContainer.InstantiatePrefabForComponent<Obstacle>(obstacleData.obstaclePrefab);
            obstacle.Initialize(obstacleData);

            return obstacle;
        }

        private static void OnGet(Obstacle obstacle)
        {
            obstacle.Reset();
            obstacle.gameObject.SetActive(true);
        }

        private static void OnRelease(Obstacle obstacle)
        {
            obstacle.gameObject.SetActive(false);
        }

        private static void OnDestroyObstacle(Obstacle obstacle)
        {
            Object.Destroy(obstacle.gameObject);
        }
    }
}
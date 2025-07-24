using UnityEngine;
using Blizzard.Obstacles;
using Blizzard.Temperature;
using System;
using ModestTree;
using Zenject;

namespace Blizzard.Obstacles
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "ScriptableObjects/Obstacles/ObstacleData")]
    public class ObstacleData : ScriptableObject
    {
        /// <summary>
        /// Prefab of the obstacle's gameobject
        /// </summary>
        public Obstacle obstaclePrefab;
        /// <summary>
        /// Initial heat value
        /// </summary>
        public float startingHeat = TemperatureConstants.DefaultHeatValue;
        /// <summary>
        /// Initial insulation value
        /// </summary>
        public float startingInsulation = TemperatureConstants.DefaultInsulationValue;
        /// <summary>
        /// Obstacle flags
        /// </summary>
        public ObstacleFlags obstacleFlags = 0;

        [Inject] DiContainer _diContainer;

        /// <summary>
        /// Creates an obstacle using 'obstaclePrefab'
        /// </summary>
        /// <param name="position">Global position to instantiate obstacle at</param>
        public virtual Obstacle CreateObstacle(Vector3 position)
        {
            Assert.That(obstaclePrefab != null, "obstaclePrefab not provided!");

            Obstacle obstacle = _diContainer.InstantiatePrefabForComponent<Obstacle>(obstaclePrefab);
            obstacle.Init(startingHeat, startingInsulation, obstacleFlags);
            obstacle.transform.position = position;

            return obstacle;
        }
    }
}

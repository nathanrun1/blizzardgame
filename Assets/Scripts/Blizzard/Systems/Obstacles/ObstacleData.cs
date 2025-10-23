using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Obstacles;
using Blizzard.Temperature;
using System;
using Blizzard.Constants;
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

        /// <summary>
        /// Which obstacle grid layer this obstacle is placed on
        /// </summary>
        public ObstacleLayer obstacleLayer;

        [Inject] private DiContainer _diContainer;

        /// <summary>
        /// Creates an obstacle using 'obstaclePrefab'
        /// </summary>
        /// <param name="position">Global position to instantiate obstacle at</param>
        public virtual Obstacle CreateObstacle(Vector3 position)
        {
            Assert.IsTrue(obstaclePrefab != null, "obstaclePrefab not provided!");

            var obstacle = _diContainer.InstantiatePrefabForComponent<Obstacle>(obstaclePrefab);
            obstacle.Init(this);
            obstacle.transform.position = position;

            return obstacle;
        }
    }
}
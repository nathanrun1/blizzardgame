using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Constants;
using Zenject;
using Blizzard.Utilities.DataTypes;
using UnityEngine.Serialization;

namespace Blizzard.Obstacles
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "ScriptableObjects/Obstacles/ObstacleData")]
    public class ObstacleData : ScriptableObject
    {
        /// <summary>
        /// Prefab of the obstacle's GameObject
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
        /// Initial obstacle flags
        /// </summary>
        [FormerlySerializedAs("obstacleFlags")] 
        public ObstacleFlags initialObstacleFlags = 0;

        /// <summary>
        /// Which obstacle grid layer this obstacle is placed on
        /// </summary>
        public ObstacleLayer obstacleLayer;
    }
}
using UnityEngine;
using Blizzard.Obstacles;

namespace Blizzard.Building
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/Building/BuildingData")]
    public class BuildingData : ScriptableObject
    {
        /// <summary>
        /// ObstacleData of the building's associated obstacle
        /// </summary>
        public ObstacleData obstacleData;

        /// <summary>
        /// Display name
        /// </summary>
        public string displayName;
    }
}

// "I am a Master Builder!" 
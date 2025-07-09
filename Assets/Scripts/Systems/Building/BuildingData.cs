using UnityEngine;
using Blizzard.Obstacles;
using Blizzard.Inventory;

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

        /// <summary>
        /// Cost to build this building
        /// </summary>
        public ItemGroupData cost;
    }
}

// "I am a Master Builder!" 
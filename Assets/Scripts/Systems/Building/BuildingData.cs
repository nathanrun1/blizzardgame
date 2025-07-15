using System.Collections.Generic;
using UnityEngine;
using Blizzard.Obstacles;
using Blizzard.Inventory;
using NUnit.Framework;

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
        public List<ItemAmountPair> cost;
    }
}

// "I am a Master Builder!" 
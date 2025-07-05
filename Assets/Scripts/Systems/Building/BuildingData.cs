using UnityEngine;
using Blizzard.Obstacles;
using Blizzard.Temperature;
using System;
using ModestTree;

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

        // public Cost cost??
    }
}

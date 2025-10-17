//using UnityEngine;
//using Blizzard.Obstacles;
//using Blizzard.Temperature;
//using System;
//using ModestTree;
//using Blizzard.Inventory;
//using NUnit.Framework;

//namespace Blizzard.Obstacles
//{
//    /// <summary>
//    /// Determines which type of tool can harvest a harvestable, interpreted as a bit field.
//    /// </summary>
//    public enum ToolType
//    {
//        Axe = 1,
//        Pickaxe = 2,
//    }

//    /// <summary>
//    /// Data for special type of obstacle that is harvestable by tools and yields resources
//    /// </summary>
//    [CreateAssetMenu(fileName = "HarvestableData", menuName = "ScriptableObjects/Obstacles/HarvestableData")]
//    public class HarvestableData : ObstacleData
//    {
//        /// <summary>
//        /// Starting health
//        /// </summary>
//        public int startingHealth;
//        /// <summary>
//        /// Resources given to the player when harvested
//        /// </summary>
//        public List<ItemAmountPair> resources;
//        /// <summary>
//        /// Type of harvestable, interpreted as a bit field
//        /// </summary>
//        [Tooltip("Axe = 1, Pickaxe = 2")]
//        public uint toolType;
            
//        /// <summary>
//        /// Creates a harvestable obstacle using 'obstaclePrefab'
//        /// </summary>
//        /// <param name="position">Global position to instantiate obstacle at</param>
//        public override Obstacle CreateObstacle(Vector3 position)
//        {
//            Obstacle obstacle = base.CreateObstacle(position);

//            Harvestable harvestable = obstacle as Harvestable;
//            Assert.That(harvestable != null, "obstaclePrefab of HarvestableData does not have Harvestable (or derived) script attached!");
//            harvestable.InitHarvestable(startingHealth, resources, toolType);

//            return obstacle;
//        }
//    }
//}

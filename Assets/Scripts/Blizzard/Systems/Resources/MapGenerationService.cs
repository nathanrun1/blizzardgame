using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Constants;
using Blizzard.Obstacles;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Logging;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using Random = System.Random;

namespace Blizzard.Resources
{
    using CompetingLocationMap = Dictionary<Vector2Int, List<ObstacleData>>;
    
    /// <summary>
    /// Manages procedural map generation. All generated obstacles are assumed to be on the main layer.
    /// </summary>
    public class MapGenerationService : IInitializable
    {
        [Inject] private ObstacleGridService _obstacleGridService;
        
        private readonly List<ResourceGenInfo> _resourcesToGenerate;
        
        public MapGenerationService(List<ResourceGenInfo> resourcesToGenerate)
        {
            _resourcesToGenerate = resourcesToGenerate;
        }
        
        public void Initialize()
        {
            BLog.Log("Initializing MapGenerationService");
            GenerateResources(_resourcesToGenerate);
        }
        
        /// <summary>
        /// Generates resources of given types over the game's map bounds
        /// </summary>
        private void GenerateResources(List<ResourceGenInfo> resourcesToGenerate)
        {
            Assert.IsTrue(resourcesToGenerate.All(r => r.obstacleData.obstacleLayer == ObstacleConstants.MainObstacleLayer));
            var resourceTypes = new HashSet<ObstacleData>(resourcesToGenerate.ConvertAll(x => x.obstacleData));
            
            int resourceGenSeed = ResourceConstants.SeededGeneration
                ? ResourceConstants.ResourceGenerationSeed
                : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            System.Random rand = new(resourceGenSeed);
            
            // Map competing positions to all resource types competing for that position
            CompetingLocationMap competingLocations = new();
            
            foreach (ResourceGenInfo resource in resourcesToGenerate)
            {
                ResourceLocationGenerator.Context locationGenContext = new ResourceLocationGenerator.Context
                {
                    spawnRange = new AABBInt2D(GameConstants.MapBounds),
                    gridCellSideLength = GameConstants.CellSideLength,
                    genInfo = resource
                };
                ResourceLocationGenerator locationGenerator = new(rand.Next(), locationGenContext);
                    
                GenerateResourcesOfType(resource.obstacleData, resourceTypes, locationGenerator, competingLocations);
            }

            foreach (var (location, competingTypes) in competingLocations)
            {
                ObstacleData resourceToPlace = competingTypes[rand.Next(competingTypes.Count)];
                _obstacleGridService.TryRemoveObstacleAt(location);
                _obstacleGridService.PlaceObstacleAt(location, resourceToPlace);
            }
        }

        /// <summary>
        /// Generates (places) resources of given type. For any already-occupied location, leaves the occupying obstacle
        /// if it is the same resource or non-resource obstacle, otherwise marks it as a competing location.
        /// </summary>
        private void GenerateResourcesOfType(ObstacleData resourceType, HashSet<ObstacleData> resourceTypes, 
            ResourceLocationGenerator locationGenerator, CompetingLocationMap competingLocations)
        {
            foreach (Vector2Int location in locationGenerator.ResourceSpawnLocations)
            {
                if (_obstacleGridService.TryGetObstacleAt(location, out Obstacle obstacle))
                {
                    if (obstacle.ObstacleData == resourceType || !resourceTypes.Contains(obstacle.ObstacleData))
                        continue; // Same resource or non-obstacle already at location, leave it as is.
                    
                    BLog.Log($"{resourceType} spawn competing with {obstacle.ObstacleData} spawn!");
                    if (!competingLocations.TryAdd(location, new List<ObstacleData> { obstacle.ObstacleData }))
                        competingLocations[location].Add(obstacle.ObstacleData);
                    competingLocations[location].Add(resourceType);
                    continue; // Competing locations will be handled separately
                }
                
                _obstacleGridService.PlaceObstacleAt(location, resourceType);
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Blizzard.Constants;
using UnityEngine;

namespace Blizzard.Obstacles
{
    // TODO: fix to work with obstaclelayers
    public static class ObstacleGridServiceExtensions
    {
        /// <summary>
        /// Cache mapping obstacle flags to list of valid positions with that flag
        /// </summary>
        static Dictionary<ObstacleFlags, List<Vector2Int>> _flagQueryCache = new Dictionary<ObstacleFlags, List<Vector2Int>>();

        static System.Random _rand = new System.Random();

        public static IEnumerable<Obstacle> GetAllObstaclesWithFlags(this ObstacleGridService obstacleGridService, ObstacleFlags obstacleFlags, ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            return obstacleGridService.Grids[obstacleLayer].Values.Where(o => (o.ObstacleFlags & obstacleFlags) == obstacleFlags);
        }

        public static IEnumerable<Vector2Int> GetAllObstacleLocationsWithFlags(this ObstacleGridService obstacleGridService, ObstacleFlags obstacleFlags, ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            return obstacleGridService.Grids[obstacleLayer].ValidPositions.Where(p => (obstacleGridService.Grids[obstacleLayer].GetAt(p).ObstacleFlags & obstacleFlags) == obstacleFlags);
        }

        /// <summary>
        /// Retrieves a random obstacle with matching obstacle flags on the main obstacle layer, or null if none exist
        /// </summary>
        public static Obstacle GetRandomObstacleWithFlags(this ObstacleGridService obstacleGridService, ObstacleFlags obstacleFlags)
        {
            if (_flagQueryCache.ContainsKey(obstacleFlags) && _flagQueryCache[obstacleFlags].Count > 3)
            {
                bool obstacleIsValid =
                    obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer].TryGetValue(_flagQueryCache[obstacleFlags][_rand.Next(_flagQueryCache[obstacleFlags].Count)], out Obstacle obstacle);
                obstacleIsValid = obstacleIsValid && obstacle.gameObject != null;
                if (obstacleIsValid) return obstacle;
            }

            // No matching cache entry or fetched obstacle no longer valid, update cache and retry
            _flagQueryCache[obstacleFlags] = obstacleGridService.GetAllObstacleLocationsWithFlags(obstacleFlags).ToList();
            if (_flagQueryCache[obstacleFlags].Count == 0) return null; // No obstacles matching flags

            return obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer].GetAt(_flagQueryCache[obstacleFlags][_rand.Next(_flagQueryCache[obstacleFlags].Count)]); // Guaranteed to be valid
        }
    }
}
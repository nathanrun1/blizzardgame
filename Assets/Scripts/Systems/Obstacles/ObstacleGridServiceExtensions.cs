using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blizzard.Obstacles
{
    public static class ObstacleGridServiceExtensions
    {
        /// <summary>
        /// Cache mapping obstacle flags to list of valid positions with that flag
        /// </summary>
        static Dictionary<ObstacleFlags, List<Vector2Int>> _flagQueryCache = new Dictionary<ObstacleFlags, List<Vector2Int>>();

        static System.Random _rand = new System.Random();

        public static IEnumerable<Obstacle> GetAllObstaclesWithFlags(this ObstacleGridService obstacleGridService, ObstacleFlags obstacleFlags)
        {
            return obstacleGridService.Grid.Values.Where(o => (o.ObstacleFlags & obstacleFlags) == obstacleFlags);
        }

        public static IEnumerable<Vector2Int> GetAllObstacleLocationsWithFlags(this ObstacleGridService obstacleGridService, ObstacleFlags obstacleFlags)
        {
            return obstacleGridService.Grid.ValidPositions.Where(p => (obstacleGridService.Grid.GetAt(p).ObstacleFlags & obstacleFlags) == obstacleFlags);
        }

        /// <summary>
        /// Retrieves a random obstacle with matching obstacle flags, or null if none exist
        /// </summary>
        public static Obstacle GetRandomObstacleWithFlags(this ObstacleGridService obstacleGridService, ObstacleFlags obstacleFlags)
        {
            if (_flagQueryCache.ContainsKey(obstacleFlags) && _flagQueryCache[obstacleFlags].Count > 3)
            {
                bool obstacleIsValid = 
                    obstacleGridService.Grid.TryGetValue(_flagQueryCache[obstacleFlags][_rand.Next(_flagQueryCache[obstacleFlags].Count)], out Obstacle obstacle);
                obstacleIsValid = obstacleIsValid && obstacle.gameObject != null;
                if (obstacleIsValid) return obstacle;
            }

            // No matching cache entry or fetched obstacle no longer valid, update cache and retry
            _flagQueryCache[obstacleFlags] = obstacleGridService.GetAllObstacleLocationsWithFlags(obstacleFlags).ToList();
            if (_flagQueryCache[obstacleFlags].Count == 0) return null; // No obstacles matching flags

            return obstacleGridService.Grid.GetAt(_flagQueryCache[obstacleFlags][_rand.Next(_flagQueryCache[obstacleFlags].Count)]); // Guaranteed to be valid
        }
    }
}
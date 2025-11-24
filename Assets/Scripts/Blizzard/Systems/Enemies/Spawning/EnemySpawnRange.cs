using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Constants;
using Blizzard.Grid;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Temperature;
using Blizzard.Utilities.Assistants;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;

namespace Blizzard.Enemies.Spawning
{
    /// <summary>
    /// A range of coordinates that are valid for enemy spawning
    /// </summary>
    public class EnemySpawnRange
    {
        [Inject] private readonly TemperatureService _temperatureService;
        [Inject] private readonly ObstacleGridService _obstacleGridService;
        [Inject] private readonly PlayerService _playerService;

        /// <summary>
        /// Set of all coordinates within min/max distance range centered around (0,0)
        /// </summary>
        private readonly List<Vector2Int> _unbiasedInitialSet = new(
            GridAssistant.GetPointsInDistanceRange(
                new Vector2Int(0, 0),
                EnemyConstants.MinSpawnDistance, 
                EnemyConstants.MaxSpawnDistance)
        );
        /// <summary>
        /// Set of valid enemy spawn locations
        /// </summary>
        private List<Vector2Int> _validLocationsSet = new();
        /// <summary>
        /// Container for the UpdateValidLocations() coroutine 
        /// </summary>
        private IEnumerator _validLocationUpdater;
        
        /// <summary>
        /// Random number generator
        /// </summary>
        private readonly System.Random _rand = new();

        /// <summary>
        /// Ticks the enemy spawn range, performing one step toward updating the current valid locations.
        /// </summary>
        public IEnumerator Tick()
        {
            _validLocationUpdater = UpdateValidLocations();
            while (true)
            {
                for (int i = 0; i < EnemyConstants.SpawnRangeUpdateIterationsPerTick; ++i)
                {
                    if (_validLocationUpdater == null || !_validLocationUpdater.MoveNext()) 
                        // Restart valid locations update
                        _validLocationUpdater = UpdateValidLocations();
                }
                yield return null;
            }
        }

        /// <summary>
        /// Retrieves a random valid enemy spawn location
        /// </summary>
        public Vector2Int GetRandomEnemySpawnLocation()
        {
            // Retrieve random locations from valid set 
            Vector2Int location = _validLocationsSet[_rand.Next(_validLocationsSet.Count)];
            while (!FilterLocation(location)) // If not valid, keep trying until valid (this can happen if most recent update is no longer valid, but will be fixed on next cycle)
                location = _validLocationsSet[_rand.Next(_validLocationsSet.Count)];
            return location;
        }
        
        /// <summary>
        /// Updates the set of valid enemy spawn locations
        /// </summary>
        private IEnumerator UpdateValidLocations()
        {
            Vector2Int playerGridPos = _obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                .WorldToCellPos(_playerService.PlayerPosition);
            List<Vector2Int> newValidLocations = new();
            foreach (Vector2Int unbiasedCoordinate in _unbiasedInitialSet)
            {
                if (FilterLocation(unbiasedCoordinate + playerGridPos)) 
                    newValidLocations.Add(unbiasedCoordinate + playerGridPos);
                yield return null;
            }

            _validLocationsSet = newValidLocations;
        }
        
        /// <summary>
        /// Determines whether a given location is a valid enemy spawn location.
        /// </summary>
        /// <returns>True if location is valid, false otherwise</returns>
        private bool FilterLocation(Vector2Int location)
        {
            // Location must be unoccupied and below threshold temperature
            return !_obstacleGridService.IsOccupied(location) &&
                   _temperatureService.GetTemperatureAt(location) <= EnemyConstants.MaxSpawnTemperature;
        }
    }
}
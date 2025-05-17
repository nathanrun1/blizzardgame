using UnityEngine;
using Blizzard.Grid;
using System;
using Blizzard.Temperature;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Manages the grid of obstacles existing in the world
    /// </summary>
    public class ObstacleGridService
    {
        /// <summary>
        /// Grid containing obstacle instances mapped by their grid positions in a world grid
        /// </summary>
        public ISparseWorldGrid<Obstacle> Grid { get; private set; }


        private TemperatureService _temperatureService;

        
        
        public ObstacleGridService(ISparseWorldGrid<Obstacle> grid, TemperatureService temperatureService)
        {
            this.Grid = grid;
            this._temperatureService = temperatureService;
        }

        
        /// <summary>
        /// Returns true if given grid position is occupied by an obstacle, false otherwise.
        /// </summary>
        public bool IsOccupied(Vector2Int gridPosition)
        {
            return Grid.TryGetValue(gridPosition, out Obstacle value);
        }

        /// <summary>
        /// Returns true and sets value to occupying obstacle if position is occupied, else returns false and sets value to default.
        /// </summary>
        public bool TryGetObstacleAt(Vector2Int gridPosition, out Obstacle value)
        {
            return Grid.TryGetValue(gridPosition, out value);
        }

        /// <summary>
        /// Places obstacle at given grid position based on given obstacle data.
        /// Pre: Given grid position is unoccupied.
        /// </summary>
        public void PlaceObstacleAt(Vector2Int gridPosition, ObstacleData obstacleData)
        {
            if (IsOccupied(gridPosition))
            {
                throw new ArgumentException($"Grid position {gridPosition} occupied!");
            }

            Obstacle obstacle = MonoBehaviour.Instantiate(obstacleData.obstaclePrefab);
            obstacle.transform.position = Grid.CellToWorldPosCenter(gridPosition);
            obstacle.SetTemperatureGetter(() =>
            {
                return _temperatureService.Grid.GetAt(gridPosition).temperature;
            });

            Grid.SetAt(gridPosition, obstacle);

            UpdateTemperatureSimData(gridPosition, obstacle);
        }

        /// <summary>
        /// If given grid position is occupied, removes obstacle from that position and destroys it. Else returns false and does nothing.
        /// </summary>
        public bool TryRemoveObstacleAt(Vector2Int gridPosition)
        {
            if (!TryGetObstacleAt(gridPosition, out Obstacle obstacle)) return false;
            obstacle.Destroy();
            Grid.RemoveAt(gridPosition);

            UpdateTemperatureSimData(gridPosition, null);

            return true;
        }


        /// <summary>
        /// Invoked whenever data relevant to temperature service is updated at a specific position (e.g. heat or insulation)
        /// </summary>
        private void UpdateTemperatureSimData(Vector2Int gridPosition, Obstacle obstacle)
        {
            TemperatureCell newTemperatureCell = _temperatureService.Grid.GetAt(gridPosition);
            if (obstacle != null)
            {
                // Update temperature data using obstacle's heat and insulation values
                newTemperatureCell.heat = obstacle.Heat;
                newTemperatureCell.insulation = obstacle.Insulation;
            }
            else
            {
                // No longer an obstacle at this grid position, use default values for heat/insulation
                newTemperatureCell.heat = TemperatureConstants.DefaultHeatValue;
                newTemperatureCell.insulation = TemperatureConstants.DefaultInsulationValue;
            }

            _temperatureService.Grid.SetAt(gridPosition, newTemperatureCell);
        }
    }
}

// "Low battery! Please recharge headset."

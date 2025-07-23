using UnityEngine;
using Blizzard.Grid;
using System;
using Blizzard.Temperature;
using Zenject;

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


        [Inject] private TemperatureService _temperatureService;

        /// <summary>
        /// Empty object that is a parent to all instantiated obstacles
        /// </summary>
        private Transform _obstaclesParent;


        public ObstacleGridService(ISparseWorldGrid<Obstacle> grid, Transform obstaclesParent = null)
        {
            this.Grid = grid;
            this._obstaclesParent = obstaclesParent;
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

            Vector3 obstaclePosition = Grid.CellToWorldPosCenter(gridPosition);

            Obstacle obstacle = obstacleData.CreateObstacle(obstaclePosition);
            obstacle.OnDestroy += () => OnObstacleDestroyed(gridPosition);
            obstacle.HeatDataUpdated += () => UpdateTemperatureSimData(gridPosition, obstacle);
            if (_obstaclesParent != null) obstacle.transform.parent = _obstaclesParent;
                
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
        /// Invoked when an obstacle at the given grid position is destroyed.
        /// Removes the obstacle from the Obstacle Grid at the given grid position if it exists
        /// </summary>
        private void OnObstacleDestroyed(Vector2Int gridPosition)
        {
            if (Grid.TryGetValue(gridPosition, out _))
            {
                Grid.RemoveAt(gridPosition);
            }
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

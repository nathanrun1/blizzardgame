using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
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
        /// Grids (organized by obstacle layer) containing obstacle instances mapped by their grid positions in a world grid
        /// </summary>
        public Dictionary<ObstacleLayer, ISparseWorldGrid<Obstacle>> Grids;

        [Inject] private TemperatureService _temperatureService;

        /// <summary>
        /// Empty object that is a parent to all instantiated obstacles
        /// </summary>
        private Transform _obstaclesParent;


        public ObstacleGridService(Dictionary<ObstacleLayer, ISparseWorldGrid<Obstacle>> grids,
            Transform obstaclesParent = null)
        {
            Assert.IsTrue(grids.Count == ObstacleConstants.ObstacleLayerCount, 
                "Amount of layers in provided grid map does not match obstacle layer count! Count: " + ObstacleConstants.ObstacleLayerCount);

            this.Grids = grids;
            this._obstaclesParent = obstaclesParent;
        }

        
        /// <summary>
        /// Returns true if given grid position is occupied by an obstacle, false otherwise.
        /// </summary>
        public bool IsOccupied(Vector2Int gridPosition, ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            return Grids[obstacleLayer].TryGetValue(gridPosition, out Obstacle value);
        }

        /// <summary>
        /// Returns true and sets value to occupying obstacle if position is occupied, else returns false and sets value to default.
        /// </summary>
        public bool TryGetObstacleAt(Vector2Int gridPosition, out Obstacle value, ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            return Grids[obstacleLayer].TryGetValue(gridPosition, out value);
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

            Vector3 obstaclePosition = Grids[obstacleData.obstacleLayer].CellToWorldPosCenter(gridPosition);

            Obstacle obstacle = obstacleData.CreateObstacle(obstaclePosition);

            foreach (Renderer renderer in obstacle.GetComponentsInChildren<Renderer>(obstacle))
            {
                renderer.sortingOrder = ObstacleConstants.ObstacleLayerSortingLayers[obstacleData.obstacleLayer];
            }

            obstacle.OnDestroy += () => OnObstacleDestroyed(gridPosition, obstacleData.obstacleLayer);
            obstacle.TemperatureDataUpdated += () => UpdateTemperatureSimData(gridPosition, obstacle);
            if (_obstaclesParent != null) obstacle.transform.parent = _obstaclesParent;

            Grids[obstacleData.obstacleLayer].SetAt(gridPosition, obstacle);

            if (obstacleData.obstacleLayer == ObstacleConstants.MainObstacleLayer) UpdateTemperatureSimData(gridPosition, obstacle);
        }

        /// <summary>
        /// If given grid position is occupied, removes obstacle from that position and destroys it. Else returns false and does nothing.
        /// </summary>
        public bool TryRemoveObstacleAt(Vector2Int gridPosition, ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            if (!TryGetObstacleAt(gridPosition, out Obstacle obstacle, obstacleLayer)) return false;
            obstacle.Destroy();
            Grids[obstacleLayer].ResetAt(gridPosition);

            if (obstacleLayer == ObstacleConstants.MainObstacleLayer) UpdateTemperatureSimData(gridPosition, null);

            return true;
        }



        /// <summary>
        /// Invoked when an obstacle at the given grid position is destroyed.
        /// Removes the obstacle from the Obstacle Grid at the given grid position if it exists
        /// </summary>
        private void OnObstacleDestroyed(Vector2Int gridPosition, ObstacleLayer obstacleLayer)
        {
            if (Grids[obstacleLayer].TryGetValue(gridPosition, out _))
            {
                Grids[obstacleLayer].ResetAt(gridPosition);
                if (obstacleLayer == ObstacleConstants.MainObstacleLayer) UpdateTemperatureSimData(gridPosition, null);
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

                Debug.Log($"Temperature sim data updated at {gridPosition}.\n\tNew heat: {obstacle.Heat}\n\tNew Insulation: {obstacle.Insulation}");
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

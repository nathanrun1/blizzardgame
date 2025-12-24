using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Constants;
using Blizzard.Temperature;
using Blizzard.Utilities.Logging;
using Blizzard.Utilities.DataTypes;
using Zenject;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Manages the grid of obstacles existing in the world
    /// </summary>
    public class ObstacleGridService
    {
        [Inject] private DiContainer _diContainer;
        
        
        /// <summary>
        /// Invoked when an obstacle is added or removed from some location.
        /// Args: (Affected grid position, Affected Obstacle Layer, Flags of added/removed obstacle)
        /// </summary>
        public event Action<Vector2Int, ObstacleLayer, ObstacleFlags> ObstacleAddedOrRemoved;

        /// <summary>
        /// Invoked when an obstacle's flags are updated
        /// Args: (Affected grid position, Affected Obstacle Layer, Flags of updated obstacle)
        /// </summary>
        public event Action<Vector2Int, ObstacleLayer, ObstacleFlags> ObstacleFlagsUpdated;


        /// <summary>
        /// Grids (organized by obstacle layer) containing obstacle instances mapped by their grid positions in a world grid
        /// </summary>
        public readonly Dictionary<ObstacleLayer, ISparseWorldGrid<Obstacle>> Grids;

        /// <summary>
        /// Quadtrees for querying obstacles with different ObstacleFlags on main obstacle layer.
        /// Must be initialized with InitQuadtree()
        /// </summary>
        public Dictionary<ObstacleFlags, ObstacleQuadtree> Quadtrees { get; private set; } = new();

        [Inject] private TemperatureService _temperatureService;

        /// <summary>
        /// Empty object that is a parent to all instantiated obstacles
        /// </summary>
        private readonly Transform _obstaclesParent;


        public ObstacleGridService(Dictionary<ObstacleLayer, ISparseWorldGrid<Obstacle>> grids,
            Transform obstaclesParent = null)
        {
            Assert.IsTrue(grids.Count == ObstacleConstants.ObstacleLayerCount,
                "Amount of layers in provided grid map does not match obstacle layer count! Count: " +
                ObstacleConstants.ObstacleLayerCount);

            Grids = grids;
            _obstaclesParent = obstaclesParent;
        }


        /// <summary>
        /// Returns true if given grid position is occupied by an obstacle, false otherwise.
        /// </summary>
        public bool IsOccupied(Vector2Int gridPosition,
            ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            return Grids[obstacleLayer].TryGetValue(gridPosition, out var value);
        }

        /// <summary>
        /// Returns true and sets value to occupying obstacle if position is occupied, else returns false and sets value to default.
        /// </summary>
        public bool TryGetObstacleAt(Vector2Int gridPosition, out Obstacle value,
            ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            return Grids[obstacleLayer].TryGetValue(gridPosition, out value);
        }

        /// <summary>
        /// Places obstacle at given grid position based on given obstacle data.
        /// Pre: Given grid position is unoccupied.
        /// </summary>
        public void PlaceObstacleAt(Vector2Int gridPosition, ObstacleData obstacleData)
        {
            BLog.Log($"Placing obstacle at {gridPosition}, layer: {obstacleData.obstacleLayer}");

            if (IsOccupied(gridPosition, obstacleData.obstacleLayer))
                throw new ArgumentException(
                    $"Grid position {gridPosition} occupied on layer {obstacleData.obstacleLayer}!");

            Vector3 obstaclePosition = Grids[obstacleData.obstacleLayer].CellToWorldPosCenter(gridPosition);
            Obstacle obstacle = InstantiateObstacle(obstacleData);
            obstacle.transform.position = obstaclePosition;

            // Set sorting order of obstacle based on layer (we assume current order is relative to layer's order & add)
            foreach (Renderer renderer in obstacle.GetComponentsInChildren<Renderer>(obstacle))
                renderer.sortingOrder += ObstacleConstants.ObstacleLayerSortingLayers[obstacleData.obstacleLayer];

            obstacle.OnDestroy += () => OnObstacleDestroyed(gridPosition, obstacleData.obstacleLayer);
            obstacle.Updated += (flagsChanged) => OnObstacleUpdated(gridPosition, obstacleData.obstacleLayer, obstacle, flagsChanged);
            if (_obstaclesParent) obstacle.transform.parent = _obstaclesParent;

            Grids[obstacleData.obstacleLayer].SetAt(gridPosition, obstacle);

            UpdateTemperatureSimData(gridPosition);
            
            if (obstacleData.obstacleLayer == ObstacleConstants.MainObstacleLayer)
            {
                AddToQuadtrees(gridPosition, obstacle.ObstacleFlags);
            }

            ObstacleAddedOrRemoved?.Invoke(gridPosition, obstacleData.obstacleLayer, obstacleData.initialObstacleFlags);
        }

        /// <summary>
        /// If given grid position is occupied, removes obstacle from that position and destroys it. Else returns false and does nothing.
        /// </summary>
        public bool TryRemoveObstacleAt(Vector2Int gridPosition,
            ObstacleLayer obstacleLayer = ObstacleConstants.MainObstacleLayer)
        {
            if (!TryGetObstacleAt(gridPosition, out var obstacle, obstacleLayer)) return false;
            obstacle.Destroy();

            return true;
        }

        /// <summary>
        /// Initializes an ObstacleQuadTree with the given obstacleFlags as filter.
        /// Once initialized, it is accessible through the QuadTrees field.
        /// 
        /// If already initialized, does nothing.
        /// </summary>
        public void InitQuadtree(ObstacleFlags obstacleFlags)
        {
            if (Quadtrees.ContainsKey(obstacleFlags)) return; // Already exists
            ObstacleQuadtree quadtree = new(Grids[ObstacleConstants.MainObstacleLayer], obstacleFlags);
            Quadtrees.Add(obstacleFlags, quadtree);
        }

        /// <summary>
        /// Retrieves ObstacleQuadTree with the given obstacleFlags as filter.
        /// Initializes it if it doesn't already exist.
        /// </summary>
        public ObstacleQuadtree GetQuadtree(ObstacleFlags obstacleFlags)
        {
            InitQuadtree(obstacleFlags);
            return Quadtrees[obstacleFlags];
        }

        
        /// <summary>
        /// Instantiates an obstacle using given ObstacleData
        /// </summary>
        /// <returns></returns>
        private Obstacle InstantiateObstacle(ObstacleData obstacleData)
        {
            Assert.IsTrue(obstacleData.obstaclePrefab, "obstaclePrefab not set!");
            Obstacle obstacle = _diContainer.InstantiatePrefabForComponent<Obstacle>(obstacleData.obstaclePrefab);
            obstacle.Initialize(obstacleData);

            return obstacle;
        }
        
        /// <summary>
        /// Invoked when an obstacle at the given grid position is destroyed.
        /// Removes the obstacle from the Obstacle Grid at the given grid position if it exists
        /// </summary>
        private void OnObstacleDestroyed(Vector2Int gridPosition, ObstacleLayer obstacleLayer)
        {
            if (Grids[obstacleLayer].TryGetValue(gridPosition, out var obstacle))
            {
                Grids[obstacleLayer].ResetAt(gridPosition);
                UpdateTemperatureSimData(gridPosition);
                if (obstacleLayer == ObstacleConstants.MainObstacleLayer)
                {
                    RemoveFromQuadtrees(gridPosition);
                }
            }

            ObstacleAddedOrRemoved?.Invoke(gridPosition, obstacleLayer, obstacle.ObstacleFlags);
        }

        /// <summary>
        /// Invoked whenever an obstacle's relevant data (to this service's functionality) is updated
        /// </summary>
        private void OnObstacleUpdated(Vector2Int gridPosition, ObstacleLayer obstacleLayer, Obstacle obstacle, bool flagsChanged)
        {
            UpdateTemperatureSimData(gridPosition);
            
            if (!flagsChanged) return;
            BLog.Log("ObstacleGridService", $"ObstacleFlags changed on obstacle at {gridPosition}");
            // Obstacle flags have changed -> Update quadtree data
            RemoveFromQuadtrees(gridPosition);
            AddToQuadtrees(gridPosition, obstacle.ObstacleFlags);
            ObstacleFlagsUpdated?.Invoke(gridPosition, obstacleLayer, obstacle.ObstacleFlags);
        }

        /// <summary>
        /// Invoked whenever data relevant to temperature service is updated at a specific position (e.g. heat or insulation).
        /// Checks presence of main and floor layer obstacles, adjusting temperature grid accordingly.
        /// </summary>
        private void UpdateTemperatureSimData(Vector2Int gridPosition)
        {
            TemperatureCell newTemperatureCell = _temperatureService.Grid.GetAt(gridPosition);
            
            // Main layer obstacles affect heat/insulation/ambient
            if (Grids[ObstacleLayer.Main].TryGetValue(gridPosition, out Obstacle mainLayerObstacle))
            {
                newTemperatureCell.heat = mainLayerObstacle.Heat;
                newTemperatureCell.insulation = mainLayerObstacle.Insulation;
                newTemperatureCell.ambient =
                    mainLayerObstacle.ObstacleFlags.HasFlag(ObstacleFlags.RemovesAmbient) ? 0 : 1;
            }
            else
            {
                newTemperatureCell.heat = TemperatureConstants.DefaultHeatValue;
                newTemperatureCell.insulation = TemperatureConstants.DefaultInsulationValue;
                newTemperatureCell.ambient = 1;
            }
            
            // Floor layer obstacles affect ambient (if floor, no ambient)
            if (Grids[ObstacleLayer.Floor].TryGetValue(gridPosition, out Obstacle floorLayerObstacle))
            {
                newTemperatureCell.ambient = Math.Min(newTemperatureCell.ambient, 0);
            }

            _temperatureService.Grid.SetAt(gridPosition, newTemperatureCell);
        }

        /// <summary>
        /// Adds grid position to all quadtrees such that the given flags contain the flags in the quadtree
        /// </summary>
        private void AddToQuadtrees(Vector2Int gridPosition, ObstacleFlags obstacleFlags)
        {
            foreach (ObstacleFlags flagCombo in Quadtrees.Keys.Where(flagCombo => (flagCombo & obstacleFlags) == flagCombo))
            {
                Quadtrees[flagCombo].Add(gridPosition);
            }
        }

        /// <summary>
        /// Removes grid position from all quadtrees (effectively only from those with the grid position)
        /// </summary>
        private void RemoveFromQuadtrees(Vector2Int gridPosition)
        {
            foreach (ObstacleFlags flagCombo in Quadtrees.Keys)
            {
                Quadtrees[flagCombo].Remove(gridPosition);
            }
        }
    }
}

// "Low battery! Please recharge headset."
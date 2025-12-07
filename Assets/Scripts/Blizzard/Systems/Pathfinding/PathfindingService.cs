using System.Collections.Generic;
using UnityEngine;
using Blizzard.Grid;
using Blizzard.Constants;
using Zenject;
using Blizzard.Obstacles;
using Blizzard.Utilities;
using Blizzard.Utilities.Logging;
using Blizzard.Utilities.DataTypes;

namespace Blizzard.Pathfinding
{
    /// <summary>
    /// Service responsible for managing NPC pathfinding
    /// </summary>
    public class PathfindingService : IInitializable
    {
        private PlayerBuildingFlowField _playerBuildingFlowField;
        /// <summary>
        /// Whether flow field is valid. If false, will be as if flow field is empty.
        /// </summary>
        private bool _flowFieldValid;

        [Inject] private ObstacleGridService _obstacleGridService;

        /// <summary>
        /// Tracks outermost bounds of obstacles with PlayerBuilt flag 
        /// </summary>
        private readonly OuterMostBounds _outerMostBoundsPlayerBuilt = new();

        public void Initialize()
        {
            BLog.Log("Pathfinding Service Initialized");

            _obstacleGridService.ObstacleAddedOrRemoved += ObstacleAddedOrRemoved;
            _obstacleGridService.ObstacleFlagsUpdated += OnObstacleFlagsUpdated;

            _playerBuildingFlowField = new PlayerBuildingFlowField(_obstacleGridService);
        }

        /// <summary>
        /// Gets next position to navigate to based on current navigator position, when navigating toward
        /// player buildings.
        /// </summary>
        /// <param name="navigatorPos">Position of navigator</param>
        /// <param name="targetObstacle">Obstacle located at target position, if any</param>
        /// /// <param name="nextTargetPos">Next target position for navigator</param>
        /// <returns>Position to navigate to</returns>
        public bool TryGetNextTargetPosition(Vector2 navigatorPos, out Vector2 nextTargetPos,
            out Obstacle targetObstacle)
        {
            var gridPosition = _obstacleGridService.GetMainGrid()
                .WorldToCellPos(navigatorPos);
            if (!TryGetNextTargetGridPosition(gridPosition, out var nextGridPosition, out targetObstacle))
            {
                nextTargetPos = default;
                targetObstacle = null;
                return false;
            }

            nextTargetPos = _obstacleGridService.GetMainGrid()
                .CellToWorldPosCenter(gridPosition);
            return true;
        }

        /// <summary>
        /// Gets next grid position to navigate to based on current navigator position, when navigating
        /// toward player buildings.
        /// </summary>
        /// <param name="navigatorGridPos">Position of navigator</param>
        /// <param name="targetObstacle">Obstacle located at target position, if any</param>
        /// <param name="nextTargetGridPos">Next target position for navigator</param>
        /// <returns>Position to navigate to</returns>
        public bool TryGetNextTargetGridPosition(Vector2Int navigatorGridPos, out Vector2Int nextTargetGridPos,
            out Obstacle targetObstacle)
        {
            if (!_flowFieldValid || !_playerBuildingFlowField.TryGetNextPos(navigatorGridPos, out nextTargetGridPos))
            {
                targetObstacle = null;
                nextTargetGridPos = default;
                return false;
            }

            _obstacleGridService.TryGetObstacleAt(nextTargetGridPos, out targetObstacle);
            return true;
        }

        /// <summary>
        /// Generates a path from the given start and end positions using A*, if one is available.
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public List<Vector2Int> GenerateAStarPath(Vector2Int startPos, Vector2Int endPos)
        {
            return new(); // TEMP
        }

        /// <summary>
        /// Invoked when an obstacle is added or removed from the obstacle grid 
        /// </summary>
        private void ObstacleAddedOrRemoved(Vector2Int pos, ObstacleLayer obstacleLayer, ObstacleFlags obstacleFlags)
        {
            // TEMP: simply recalculate entire flow field
            var added = _obstacleGridService.IsOccupied(pos);

            // Add/Remove position from outermost bounds tracking
            if (obstacleFlags.HasFlag(ObstacleFlags.PlayerBuilt))
            {
                if (added) _outerMostBoundsPlayerBuilt.Add(pos);
                else _outerMostBoundsPlayerBuilt.Remove(pos);
            }

            if (_outerMostBoundsPlayerBuilt.IsEmpty())
            {
                // No player built obstacles, no need for flow field
                _flowFieldValid = false;
                return;
            } 

            // Add padding to min/max bounds so that enemies near (within padding) player built objects can still
            //  pathfind to them
            Vector2Int paddedMin = new(
                _outerMostBoundsPlayerBuilt.MinBound.x - PathfindingConstants.ffPadding,
                _outerMostBoundsPlayerBuilt.MinBound.y - PathfindingConstants.ffPadding);
            Vector2Int paddedMax = new(
                _outerMostBoundsPlayerBuilt.MinBound.x + PathfindingConstants.ffPadding,
                _outerMostBoundsPlayerBuilt.MinBound.y + PathfindingConstants.ffPadding);

            _playerBuildingFlowField.BuildFlowField(paddedMin, paddedMax);
            _flowFieldValid = true;
        }

        /// <summary>
        /// Invoked when an obstacle has its flags updated
        /// </summary>
        private void OnObstacleFlagsUpdated(Vector2Int pos, ObstacleLayer obstacleLayer, ObstacleFlags obstacleFlags)
        {
            // TEMP: simply recalculate entire flow field
            
            if (_outerMostBoundsPlayerBuilt.IsEmpty())
            {
                // No player built obstacles, no need for flow field
                _flowFieldValid = false;
                return;
            }

            Vector2Int paddedMin = new(
                _outerMostBoundsPlayerBuilt.MinBound.x - PathfindingConstants.ffPadding,
                _outerMostBoundsPlayerBuilt.MinBound.y - PathfindingConstants.ffPadding);
            Vector2Int paddedMax = new(
                _outerMostBoundsPlayerBuilt.MinBound.x + PathfindingConstants.ffPadding,
                _outerMostBoundsPlayerBuilt.MinBound.y + PathfindingConstants.ffPadding);

            _playerBuildingFlowField.BuildFlowField(paddedMin, paddedMax);
            _flowFieldValid = true;
        }
    }
}

// "Welcome... to the Amazing Brentwood!!!"
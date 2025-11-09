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
    /// Service responsible for managing enemy pathfinding
    /// </summary>
    public class EnemyPathfindingService : IInitializable
    {
        private FlowField _flowField;
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

            _obstacleGridService.OnObstacleAddedOrRemoved += OnObstacleAddedOrRemoved;
            _obstacleGridService.OnQuadtreeUpdate += OnQuadtreeUpdate;

            _flowField = new FlowField(_obstacleGridService);
        }

        /// <summary>
        /// Gets next position to navigate to based on current navigator position
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
        /// Gets next grid position to navigate to based on current navigator position
        /// </summary>
        /// <param name="navigatorGridPos">Position of navigator</param>
        /// <param name="targetObstacle">Obstacle located at target position, if any</param>
        /// <param name="nextTargetGridPos">Next target position for navigator</param>
        /// <returns>Position to navigate to</returns>
        public bool TryGetNextTargetGridPosition(Vector2Int navigatorGridPos, out Vector2Int nextTargetGridPos,
            out Obstacle targetObstacle)
        {
            if (!_flowFieldValid || !_flowField.TryGetNextPos(navigatorGridPos, out nextTargetGridPos))
            {
                targetObstacle = null;
                nextTargetGridPos = default;
                return false;
            }

            _obstacleGridService.TryGetObstacleAt(nextTargetGridPos, out targetObstacle);
            return true;
        }


        /// <summary>
        /// Invoked with the obstacle grid service's identically named event
        /// </summary>
        private void OnObstacleAddedOrRemoved(Vector2Int pos, ObstacleLayer obstacleLayer, ObstacleFlags obstacleFlags)
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

            _flowField.BuildFlowField(paddedMin, paddedMax);
            _flowFieldValid = true;
        }

        /// <summary>
        /// Invoked with the obstacle grid service's identically named event
        /// </summary>
        private void OnQuadtreeUpdate(ObstacleFlags quadtreeObstacleFlags)
        {
            BLog.Log("EnemyPathfindingService",$"Quadtree update detected for flags {quadtreeObstacleFlags.ToString()}");
            if (quadtreeObstacleFlags != ObstacleFlags.PlayerBuilt) return;  // Only relevant if player-built QT is updated
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

            _flowField.BuildFlowField(paddedMin, paddedMax);
            _flowFieldValid = true;
        }
    }
}

// "Welcome... to the Amazing Brentwood!!!"
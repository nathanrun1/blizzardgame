using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.Constants;
using Blizzard.Temperature;
using Zenject;
using FlowFieldAI;
using Blizzard.Obstacles;
using Blizzard.Utilities;
using Unity.Cinemachine;

namespace Blizzard.Pathfinding
{
    /// <summary>
    /// Responsible for pathfinding to/around obstacles.
    /// </summary>
    public class PathfindingService : IInitializable
    {
        private FlowField _flowField;

        [Inject] private ObstacleGridService _obstacleGridService;

        /// <summary>
        /// Tracks outermost bounds of obstacles with PlayerBuilt flag 
        /// </summary>
        private OuterMostBounds _outerMostBoundsPlayerBuilt = new();

        public void Initialize()
        {
            Debug.Log("Pathfinding Service Initialized");

            _obstacleGridService.OnObstacleAddedOrRemoved += OnObstacleAddedOrRemoved;

            _flowField = new FlowField(_obstacleGridService);
        }

        /// <summary>
        /// Gets next position to navigate to based on current navigator position
        /// </summary>
        /// <param name="navigatorPos">Position of navigator</param>
        /// <param name="targetObstacle">Obstacle located at target position, if any</param>
        /// /// <param name="nextTargetPos">Next target position for navigator</param>
        /// <returns>Position to navigate to</returns>
        public bool TryGetNextTargetPosition(Vector2 navigatorPos, out Vector2 nextTargetPos, out Obstacle targetObstacle)
        {
            Vector2Int gridPosition = _obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                .WorldToCellPos(navigatorPos); 
            if (!TryGetNextTargetGridPosition(gridPosition, out Vector2Int nextGridPosition, out targetObstacle))
            {
                nextTargetPos = default;
                targetObstacle = null;
                return false;
            }
            nextTargetPos = _obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer].CellToWorldPosCenter(gridPosition);
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
            if (!_flowField.TryGetNextPos(navigatorGridPos, out nextTargetGridPos))
            {
                targetObstacle = null;
                nextTargetGridPos = default;
                return false;
            }
            _obstacleGridService.TryGetObstacleAt(nextTargetGridPos, out targetObstacle);
            // Debug.Log($"Next pos is {nextTargetGridPos}, this pos is {navigatorGridPos} obstacle?: {targetObstacle}");
            return true;
        }


        /// <summary>
        /// Invoked with the obstacle grid service's identically named event
        /// </summary>
        private void OnObstacleAddedOrRemoved(Vector2Int pos, ObstacleLayer obstacleLayer, ObstacleFlags obstacleFlags)
        {
            // TEMP: simply recalculate entire flow field
            bool added = _obstacleGridService.IsOccupied(pos);
            
            // Add/Remove position from outermost bounds tracking
            if (obstacleFlags.HasFlag(ObstacleFlags.PlayerBuilt))
            {
                if (added) _outerMostBoundsPlayerBuilt.Add(pos);
                else _outerMostBoundsPlayerBuilt.Remove(pos);
            }

            if (_outerMostBoundsPlayerBuilt.IsEmpty()) return; // No player built obstacles, no need for flow field

            // Add padding to min/max bounds so that enemies near (within padding) player built objects can still
            //  pathfind to them
            Vector2Int paddedMin = new(
                _outerMostBoundsPlayerBuilt.MinBound.x - PathfindingConstants.ffPadding,
                _outerMostBoundsPlayerBuilt.MinBound.y - PathfindingConstants.ffPadding);
            Vector2Int paddedMax = new(
                _outerMostBoundsPlayerBuilt.MinBound.x + PathfindingConstants.ffPadding,
                _outerMostBoundsPlayerBuilt.MinBound.y + PathfindingConstants.ffPadding);
            
            //Debug.Log($"Building flow field with bounds {paddedMin} -> {paddedMax}");
            _flowField.BuildFlowField(paddedMin, paddedMax);
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetFullFlowFieldBounds()
        {
            
        }

        //public NativeFlowField GetFlowField(Vector2Int source, Vector2Int target)
        //{
        //    // TODO: flowfield cache

        //    // Flow field will cover a square centered on target, side length 2 * largest
        //    //   of x or y distance from source to target.
        //    // This ensures flow field 
        //    int halfSideLength = Math.Max(Math.Abs(source.x - target.x), Math.Abs(source.y - target.y));
        //}\
    }
}

// "Welcome... to the Amazing Brentwood!!!"

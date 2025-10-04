using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.Temperature;
using Zenject;
using FlowFieldAI;
using Blizzard.Obstacles;

namespace Blizzard.Pathfinding
{
    /// <summary>
    /// Responsible for pathfinding to/around obstacles.
    /// </summary>
    public class PathfindingService : IInitializable
    {
        private FlowField _flowField;


        [Inject] private ObstacleGridService _obstacleGridService;

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
        /// <returns>Position to navigate to</returns>
        public Vector2 GetNextTargetPosition(Vector2 navigatorPos, out Obstacle targetObstacle)
        {
            Vector2Int gridPosition = _obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                .WorldToCellPos(navigatorPos);
            Vector2Int nextGridPosition = GetNextTargetGridPosition(gridPosition, out targetObstacle);
            return nextGridPosition;
        }

        /// <summary>
        /// Gets next grid position to navigate to based on current navigator position
        /// </summary>
        /// <param name="navigatorGridPos">Position of navigator</param>
        /// <param name="targetObstacle">Obstacle located at target position, if any</param>
        /// <returns>Position to navigate to</returns>
        public Vector2Int GetNextTargetGridPosition(Vector2Int navigatorGridPos, out Obstacle targetObstacle)
        {
            Vector2Int nextGridPosition = _flowField.GetNextPos(navigatorGridPos);
            _obstacleGridService.TryGetObstacleAt(nextGridPosition, out targetObstacle);
            return nextGridPosition;
        }


        /// <summary>
        /// Invoked with the obstacle grid service's identically named event
        /// </summary>
        private void OnObstacleAddedOrRemoved(Vector2Int pos, ObstacleLayer obstacleLayer)
        {
            // TEMP: simply recalculate entire flow field

            // TEMP: hardcoded min/max bounds
            Vector2Int min = new(-50, -50);
            Vector2Int max = new(50, 50);

            _flowField.BuildFlowField(min, max);
        }

        //public NativeFlowField GetFlowField(Vector2Int source, Vector2Int target)
        //{
        //    // TODO: flowfield cache

        //    // Flow field will cover a square centered on target, side length 2 * largest
        //    //   of x or y distance from source to target.
        //    // This ensures flow field 
        //    int halfSideLength = Math.Max(Math.Abs(source.x - target.x), Math.Abs(source.y - target.y));
        //}


        /// <summary>
        /// Performs A* pathfinding from given start to end points.
        /// Operates over the main obstacle layer.
        /// </summary>
        /// <returns>List of coordinates cooresponding to retrieved path</returns>
        //private List<Vector2Int> AStar(Vector2Int start, Vector2Int end)
        //{

        //}
    }
}

// "Welcome... to the Amazing Brentwood!!!"

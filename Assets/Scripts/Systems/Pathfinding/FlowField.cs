using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Obstacles;
using FlowFieldAI;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Blizzard.Pathfinding
{
    enum NbrLocation
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        UpLeft = 4,
        UpRight = 5,
        DownRight = 6,
        DownLeft = 7
    }

    /// <summary>
    /// Represents a contained flow field region.
    /// </summary>
    class FlowFieldChunk
    {
        /// <summary>
        /// Underlying native flow field for this chunk
        /// </summary>
        NativeFlowField _nativeFlowField;


        /// <summary>
        /// Flattened flow field data
        /// </summary>
        public float[] flowField;

        public FlowFieldChunk()
        {
            _nativeFlowField = new NativeFlowField(
                PathfindingConstants.ffChunkSideLength, 
                PathfindingConstants.ffChunkSideLength
                );
        }

        public void ConstructFlowFieldData(Vector2Int chunkPosition, PathfindTargetType targetType,
            ObstacleGridService obstacleGridService)
        {
            // Calculate min/max bounds of flow field in grid.
            // Note: We add +1 padding to each side for neighboring cells
            Vector2Int minBounds = new(
                chunkPosition.x * PathfindingConstants.ffChunkSideLength - 1,
                chunkPosition.x * PathfindingConstants.ffChunkSideLength - 1
                );
            Vector2Int maxBounds = new(
                minBounds.x + PathfindingConstants.ffChunkSideLength + 2,
                minBounds.y + PathfindingConstants.ffChunkSideLength + 2
                );


            // Default initialize to float.MinValue (i.e. "Walkable")
            flowField = Enumerable.Repeat(float.MinValue,
                (PathfindingConstants.ffChunkSideLength + 2) * (PathfindingConstants.ffChunkSideLength + 2))
                .ToArray();

            if (targetType == PathfindTargetType.PlayerBuildings)
            { 
                List<Vector2Int> inRange = obstacleGridService.GetQuadtree((ObstacleFlags)0)
                    .GetValidPositionsInRange(minBounds, maxBounds);
                foreach (Vector2Int pos in inRange)
                {
                    Obstacle obstacle = obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                        .GetAt(pos);
                    Assert.IsNotNull(obstacle, "Failed to construct flow field chunk, queried a null obstacle!");

                    // TODO: improve weight calculation based on obstacle
                    flowField[Flatten(pos, minBounds)] = 0; // TEMP: Set to default target value (0)
                }
            }
        }

        /// <summary>
        /// Calculates flattened index within chunk of given grid position
        /// </summary>
        private int Flatten(Vector2Int position, Vector2Int minBounds)
        {
            return (position.x - minBounds.x) +
                (position.y - minBounds.y) * (PathfindingConstants.ffChunkSideLength + 2);
        }
    }

    /// <summary>
    /// Type of pathfinding target. A flow field can be requested for each type.
    /// </summary>
    public enum PathfindTargetType
    {
        PlayerBuildings
    }

    /// <summary>
    /// A Flow Field, allows retrieval of navigation direction for given
    /// pathfinding target type.
    /// </summary>
    public class FlowField
    {
        /// <summary>
        /// Underlying NativeFlowField implementation
        /// </summary>
        private NativeFlowField _nativeFlowField;
        /// <summary>
        /// Current minimum bound of native flow field
        /// </summary>
        private Vector2Int _curMinBound;
        /// <summary>
        /// Flattened flowfield input data (i.e. weights of each grid position)
        /// </summary>
        private NativeArray<float> _inField;


        private ObstacleGridService _obstacleGridService;

        public FlowField(ObstacleGridService obstacleGridService)
        {
            _obstacleGridService = obstacleGridService;
        }
        

        /// <summary>
        /// Builds flow field for given min & max bounds
        /// </summary>
        /// <param name="min">Inclusive minimum bound</param>
        /// <param name="max">Inclusive maximum bound</param>
        public void BuildFlowField(Vector2Int min, Vector2Int max)
        {
            int width = (max.x - min.x) + 1; // Add 1 because bounds are inclusive
            int height = (max.y - min.y) + 1; // ^^
            Debug.Log($"Rebuilding flow field of size {width} * {height}!");

            if (_nativeFlowField == null || _nativeFlowField.Width != width || _nativeFlowField.Height != height)
            {
                // Correct the flow field's dimensions
                _nativeFlowField = new NativeFlowField(width, height);
            }

            // Default initialize to float.MinValue (i.e. "Walkable")
            float[] _inFieldArr = Enumerable.Repeat(float.MinValue, width * height).ToArray();

            List<Vector2Int> inRange = _obstacleGridService.GetQuadtree((ObstacleFlags)0)
                    .GetValidPositionsInRange(min, max);
            //Debug.Log($"Obstacles in range: {inRange.Count}");
            foreach (Vector2Int pos in inRange)
            {
                Obstacle obstacle = _obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                    .GetAt(pos);
                Assert.IsNotNull(obstacle, "Failed to construct flow field, queried a null obstacle!");

                // TODO: improve weight calculation based on obstacle
                Debug.Log($"Adding obstacle at position {pos}");
                _inFieldArr[Flatten(pos, min, width)] = 0; // TEMP: Set to default target value (0)
            }

            if (_inField.IsCreated) _inField.Dispose(); // Free existing data (persistent allocation)
            _inField = new NativeArray<float>(_inFieldArr, Allocator.Persistent);

            _nativeFlowField.Bake(_inField, new BakeOptions
            {
                Iterations = 100,
                IterationsPerFrame = 0,
                DiagonalMovement = true,
                ComputeQueueType = UnityEngine.Rendering.ComputeQueueType.Background
            });
            _curMinBound = min;
        }

        /// <summary>
        /// Retrieves the next grid position to travel to based on the given
        /// current grid position
        /// </summary>
        /// <param name="pos">Current grid position of navigator</param>
        public Vector2Int GetNextPos(Vector2Int pos)
        {
            int cellIndex = Flatten(pos, _curMinBound, _nativeFlowField.Width);
            int nextIndex = _nativeFlowField.NextIndices[cellIndex];
            return Unflatten(nextIndex, _curMinBound, _nativeFlowField.Width);
        }

        /// <summary>
        /// Calculates flattened index from 2D position
        /// </summary>
        /// <param name="position">Position in 2D space</param>
        /// <param name="minBounds">Bottom left (minimum) of 2D space</param>
        /// <param name="width">Width of 2D space</param>
        /// <returns>Flattened index</returns>
        private int Flatten(Vector2Int position, Vector2Int minBounds, int width)
        {
            return (position.x - minBounds.x) +
                (position.y - minBounds.y) * width;
        }

        /// <summary>
        /// Unflattens flattened index back to corresponding 2D position
        /// </summary>
        /// <param name="index">Flattened index</param>
        /// <param name="minBounds">Bottom left (minimum) of 2D space</param>
        /// <param name="width">Width of 2D space</param>
        /// <returns>Corresponding 2D position</returns>
        private Vector2Int Unflatten(int index, Vector2Int minBounds, int width)
        {
            return new Vector2Int((index % width) + minBounds.x, (index / width) + minBounds.y);
        }
    }
}
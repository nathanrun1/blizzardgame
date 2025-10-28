using System.Linq;
using Blizzard.Constants;
using Blizzard.Obstacles;
using FlowFieldAI;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using Blizzard.Utilities.Logging;

namespace Blizzard.Pathfinding
{
    /// <summary>
    /// Represents a contained flow field region.
    /// </summary>
    internal class FlowFieldChunk
    {
        /// <summary>
        /// Underlying native flow field for this chunk
        /// </summary>
        private NativeFlowField _nativeFlowField;


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
                var inRange = obstacleGridService.GetQuadtree((ObstacleFlags)0)
                    .GetValidPositionsInRange(minBounds, maxBounds);
                foreach (var pos in inRange)
                {
                    var obstacle = obstacleGridService.GetMainGrid()
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
            return position.x - minBounds.x +
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
        /// Current maximum bound of native flow field
        /// </summary>
        private Vector2Int _curMaxBound;

        /// <summary>
        /// Flattened flow field input data (i.e. weights of each grid position)
        /// </summary>
        private NativeArray<float> _inField;

        /// <summary>
        /// Flattened travel costs input data (i.e. travel cost of each grid position)
        /// </summary>
        private NativeArray<float> _travelCosts;

        /// <summary>
        /// Lock for flow field construction
        /// </summary>
        private readonly object _ffBuildLock = new();


        private readonly ObstacleGridService _obstacleGridService;

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
            lock (_ffBuildLock)
            {
                var width = max.x - min.x + 1; // Add 1 because bounds are inclusive
                var height = max.y - min.y + 1; // ^^
                BLog.Log($"Rebuilding flow field of size {width} * {height}!");

                if (_nativeFlowField == null || _nativeFlowField.Width != width || _nativeFlowField.Height != height)
                    // Correct the flow field's dimensions (or initialize if not already)
                    _nativeFlowField = new NativeFlowField(width, height, useTravelCosts: true);

                // Default initialize to float.MinValue (i.e. "Walkable")
                float[] inFieldArr = Enumerable.Repeat(float.MinValue, width * height).ToArray();
                float[] travelCostsArr = Enumerable.Repeat(0f, width * height).ToArray();

                var inRange = _obstacleGridService.GetQuadtree((ObstacleFlags)0)
                    .GetValidPositionsInRange(min, max);
                //BLog.Log($"Obstacles in range: {inRange.Count}");
                foreach (var pos in inRange)
                {
                    var obstacle = _obstacleGridService.GetMainGrid()
                        .GetAt(pos);
                    Assert.IsNotNull(obstacle, "Failed to construct flow field, queried a null obstacle!");

                    // Check if player built
                    bool isPlayerBuilt = (ObstacleFlags.PlayerBuilt & obstacle.ObstacleFlags) ==
                                         ObstacleFlags.PlayerBuilt;

                    // TODO: improve weight calculation based on obstacle
                    float weight = isPlayerBuilt ? 0f : float.MinValue; // Set no weight if not player built
                    float travelCost =
                        (obstacle as Damageable)
                            ? (obstacle as Damageable)!.Health / 10f
                            : 0f; // TEMP: set travel cost to health/10, though this should depend on enemy's dps

                    inFieldArr[Flatten(pos, min, width)] = weight; // TEMP: Set to default target value (0)
                    travelCostsArr[Flatten(pos, min, height)] = travelCost;

                    //BLog.Log($"Adding obstacle at position {pos} with weight {weight}, travel cost {travelCost}");
                }

                if (_inField.IsCreated) _inField.Dispose(); // Free existing data (persistent allocation)
                if (_travelCosts.IsCreated) _travelCosts.Dispose(); // ^^

                _inField = new NativeArray<float>(inFieldArr, Allocator.Persistent);
                _travelCosts = new NativeArray<float>(travelCostsArr, Allocator.Persistent);
                var rq = _nativeFlowField.Bake(_inField, _travelCosts, new BakeOptions // TODO: Async
                {
                    Iterations = 100,
                    IterationsPerFrame = 0,
                    DiagonalMovement = false,
                    ComputeQueueType = ComputeQueueType.Background
                });
                rq.WaitForCompletion();
                _curMinBound = min;
                _curMaxBound = max;
            }
        }

        /// <summary>
        /// Attempts to retrieve the next grid position to travel to based on the given
        /// current grid position. Successfully does so if given position is within flow field.
        /// </summary>
        /// <param name="pos">Current grid position of navigator</param>
        /// <param name="nextPos">Next destination grid position for navigator</param>
        /// <returns>Whether next position successfully retrieved</returns>
        public bool TryGetNextPos(Vector2Int pos, out Vector2Int nextPos)
        {
            if (pos.x < _curMinBound.x || pos.y < _curMinBound.y || pos.x > _curMaxBound.x || pos.y > _curMaxBound.y)
            {
                // Out of range
                nextPos = default;
                return false;
            }

            var cellIndex = Flatten(pos, _curMinBound, _nativeFlowField.Width);
            var nextIndex = _nativeFlowField.NextIndices[cellIndex];
            nextPos = Unflatten(nextIndex, _curMinBound, _nativeFlowField.Width);
            return true;
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
            // BLog.Log($"Flattening position {position} to {(position.x - minBounds.x) + (position.y - minBounds.y) * width}");
            return position.x - minBounds.x +
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
            // BLog.Log($"Unflattening index {index} to {new Vector2Int((index % width) + minBounds.x, (index / width) + minBounds.y)}");
            return new Vector2Int(index % width + minBounds.x, index / width + minBounds.y);
        }
    }
}
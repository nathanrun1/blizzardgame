using NativeTrees;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Blizzard.Grid;
using System.Linq;
using Blizzard.Constants;
using Blizzard.Utilities.Quadtree;
using Blizzard.Utilities.DataTypes;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Wrapper for QuadTree storing obstacles
    /// </summary>
    public class ObstacleQuadtree
    {
        /// <summary>
        /// Represents a stored obstacle in a NativeQuadTree
        /// </summary>
        private struct QTObstacleData
        {
            /// <summary>
            /// Position of associated obstacle in obstacle grid
            /// </summary>
            public readonly Vector2Int position;

            public QTObstacleData(Vector2Int position)
            {
                this.position = position;
            }
        }
        
        /// <summary>
        /// Underlying QuadTree implementation
        /// </summary>
        private NativeQuadtree<QTObstacleData> _nativeQuadTree;

        private bool _quadTreeInitialized = false;

        /// <summary>
        /// Set of coordinates that have been inserted into the underlying quadtree,
        /// yet the associated obstacle has been removed.
        /// </summary>
        private readonly HashSet<Vector2Int> _invalidPositions = new();

        /// <summary>
        /// Associated obstacle grid, will cross-reference obstacles from within
        /// for queries.
        /// </summary>
        private readonly ISparseWorldGrid<Obstacle> _obstacleGrid;

        /// <summary>
        /// Obstacle flags used as a filter when refreshing the QuadTree
        /// </summary>
        private readonly ObstacleFlags _obstacleFlags;


        /// <summary>
        /// Initializes an ObstacleQuadTree
        /// </summary>
        /// <param name="obstacleGrid">Associated obstacle grid</param>
        /// <param name="obstacleFlags">Obstacle flag filters</param>
        public ObstacleQuadtree(ISparseWorldGrid<Obstacle> obstacleGrid, ObstacleFlags obstacleFlags)
        {
            _obstacleGrid = obstacleGrid;
            _obstacleFlags = obstacleFlags;
        }


        /// <summary>
        /// Adds given obstacle coordinate to QuadTree so it can be queried.
        /// </summary>
        public void Add(Vector2Int obstaclePosition)
        {
            // BLog.Log($"Adding {obstaclePosition} to quad tree (flags: {_obstacleFlags})");
            if (!_quadTreeInitialized) Rebuild();
            if (!_nativeQuadTree.Bounds.Contains(new float2(obstaclePosition.x, obstaclePosition.y)))
                Rebuild(); // New point not contained within quadtree, must rebuild.

            if (_invalidPositions.Contains(obstaclePosition))
            {
                // Obstacle no longer invalid, we can simply remove it from this set.
                _invalidPositions.Remove(obstaclePosition);
                return;
            }

            _nativeQuadTree.InsertPoint(
                new QTObstacleData(obstaclePosition),
                new float2(obstaclePosition.x, obstaclePosition.y)
            );
        }

        /// <summary>
        /// Removes given obstacle coordinate from valid coordinates in QuadTree.
        /// This coordinate can still be queried until the QuadTree is refreshed, 
        /// returning a null Obstacle.
        ///
        /// Has no effect if coordinate is not in the quadtree.
        /// </summary>
        /// <param name="obstaclePosition"></param>
        public void Remove(Vector2Int obstaclePosition)
        {
            if (!_quadTreeInitialized) Rebuild();

            // Invalidate the given coordinate
            _invalidPositions.Add(obstaclePosition);
        }

        /// <summary>
        /// Retrieves up to K nearest obstacles to given position. May return less than k
        /// if k is larger than total obstacles within max distance or if more than the
        /// maximum allowed percentage are invalid.
        /// </summary>
        /// <returns>List of K nearest obstacles.</returns>
        public List<Obstacle> GetKNearestObstacles(Vector2Int position, int k, int maxDistance)
        {
            if (!_quadTreeInitialized) Rebuild();

            // BLog.Log($"[ObstacleQuadTree] Querying {k} nearest to {position}...");
            var visitor = new QTKNearestVisitor<QTObstacleData>(k);
            float2 point = new(position.x, position.y);
            _nativeQuadTree.Nearest(point, maxDistance,
                ref visitor, default(QTManhattanDistanceProvider<QTObstacleData>));

            List<Obstacle> queryResults = new(k);
            int invalidCount = 0;
            foreach (var data in visitor.kNearest)
            {
                // BLog.Log($"[ObstacleQuadTree] Queried obstacle at pos {data.position}. Checking if valid...");
                if (_invalidPositions.Contains(data.position))
                {
                    invalidCount++;

                    // Check if invalid threshold reached
                    if (invalidCount / (float)k >= ObstacleConstants.QTMaxAllowedInvalidInQuery)
                    {
                        // Too many invalid retrieved in query! Rebuild and then restart query.
                        Rebuild();
                        return GetKNearestObstacles(position, k, maxDistance);
                    }

                    // Not reached, simply omit it.
                    continue;
                }

                var obstacle = _obstacleGrid.GetAt(data.position);
                Assert.IsTrue(obstacle, "QTObstacleData not marked invalid, yet obstacle" +
                                        "doesn't exist in grid!");

                queryResults.Add(obstacle);
            }

            // BLog.Log($"[ObstacleQuadTree] Successfully queried {queryResults.Count} obstacles!");
            return queryResults;
        }

        /// <summary>
        /// Retrieves all obstacles within a given range.
        /// </summary>
        /// <param name="min">Inclusive minimum of range</param>
        /// <param name="max">Inclusive maximum of range</param>
        /// <returns>List of queried obstacles within the given range</returns>
        public List<Obstacle> GetObstaclesInRange(Vector2Int min, Vector2Int max)
        {
            List<Obstacle> results = new();

            var validPositions = GetValidPositionsInRange(min, max);
            foreach (var pos in validPositions)
            {
                var obstacle = _obstacleGrid.GetAt(pos);
                Assert.IsTrue(obstacle != null, "QTObstacleData not marked invalid, yet obstacle" +
                                                "doesn't exist in grid!");
                results.Add(obstacle);
            }

            return results;
        }

        /// <summary>
        /// Retrieves all valid obstacle positions within a given range.
        /// </summary>
        /// <param name="min">Inclusive minimum of range</param>
        /// <param name="max">Inclusive maximum of range</param>
        /// <returns>List of queried obstacle positions that are valid within the given range</returns>
        public List<Vector2Int> GetValidPositionsInRange(Vector2Int min, Vector2Int max)
        {
            if (!_quadTreeInitialized) Rebuild();

            var visitor = new QTRangeVisitor<QTObstacleData>(min, max);

            // Add 0.5f padding to range so that it includes points on the edge (since they are integer coords)
            float2 flMin = new((float)min.x - 0.5f, (float)min.y - 0.5f);
            float2 flMax = new((float)max.x + 0.5f, (float)max.y + 0.5f);
            AABB2D range = new(flMin, flMax);

            _nativeQuadTree.Range(range, ref visitor);

            List<Vector2Int> results = new();
            //BLog.Log($"Range query ({flMin}, {flMax}) yielded {visitor.results.Count} results.");
            foreach (var data in visitor.results)
            {
                if (_invalidPositions.Contains(data.position)) continue; // Position is invalid
                results.Add(data.position);
            }

            return results;
        }

        /// <summary>
        /// Rebuilds contents of the QuadTree, clearing all invalid entries.
        /// </summary>
        private void Rebuild()
        {
            // BLog.Log("[ObstacleQuadTree] Rebuilding!");

            _invalidPositions.Clear();
            if (_obstacleGrid.Empty()) return; // Nothing in grid yet
            var firstPos = _obstacleGrid.ValidPositions.First();
            float2 minPos = new(firstPos.x, firstPos.y);
            float2 maxPos = new(firstPos.x, firstPos.y);
            foreach (var position in _obstacleGrid.ValidPositions)
            {
                if (position.x < minPos.x) minPos = new float2(position.x, minPos.y);
                if (position.y < minPos.y) minPos = new float2(minPos.x, position.y);
                if (position.x > maxPos.x) maxPos = new float2(position.x, maxPos.y);
                if (position.y > maxPos.y) maxPos = new float2(maxPos.x, position.y);
            }

            float boundsWidth = maxPos.x - minPos.x;
            float boundsHeight = maxPos.y - minPos.y;

            float horizontalPadding = boundsWidth * ObstacleConstants.QTBoundsPadding;
            float verticalPadding = boundsHeight * ObstacleConstants.QTBoundsPadding;

            minPos = new float2(minPos.x - horizontalPadding, minPos.y - verticalPadding);
            maxPos = new float2(maxPos.x + horizontalPadding, maxPos.y + verticalPadding);

            AABB2D bounds = new(minPos, maxPos);

            // Re-initialize quadtree
            _nativeQuadTree = new NativeQuadtree<QTObstacleData>(bounds, ObstacleConstants.QTObjectsPerNode,
                ObstacleConstants.QTMaxDepth, Unity.Collections.Allocator.Persistent, _obstacleGrid.Count);
            _quadTreeInitialized = true;
            foreach (var (position, obstacle) in _obstacleGrid)
                if ((obstacle.ObstacleFlags & _obstacleFlags) == _obstacleFlags)
                    // Filter met, add to quadtree
                    Add(position);
        }
    }
}
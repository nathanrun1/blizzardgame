using NativeTrees;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Blizzard.Grid;
using System.Linq;
using Blizzard.Constants;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Wrapper for QuadTree storing obstacles
    /// </summary>
    public class ObstacleQuadTree
    {
        #region QT Structs

        /// <summary>
        /// Represents a stored obstacle in a NativeQuadTree
        /// </summary>
        private struct QTObstacleData
        {
            /// <summary>
            /// Position of associated obstacle in obstacle grid
            /// </summary>
            public Vector2Int position;

            public QTObstacleData(Vector2Int position)
            {
                this.position = position;
            }
        }

        private struct QTManhattanDistanceProvider : IQuadtreeDistanceProvider<QTObstacleData>
        {
            public float DistanceSquared(float2 point, QTObstacleData _, AABB2D bounds)
            {
                return Mathf.Abs(point.x - bounds.Center.x) +
                       Mathf.Abs(point.y - bounds.Center.y);
            }
        }

        /// <summary>
        /// QuadTree Nearest Visitor that provides the K nearest obstacles
        /// </summary>
        private struct QTKNearestVisitor : IQuadtreeNearestVisitor<QTObstacleData>
        {
            public List<QTObstacleData> kNearest;

            private int _k;

            public bool OnVist(QTObstacleData obj)
            {
                kNearest.Add(obj);

                // Continue iterating until k found
                return kNearest.Count < _k;
            }

            public QTKNearestVisitor(int k)
            {
                kNearest = new List<QTObstacleData>();

                _k = k;
            }
        }

        private struct QTRangeVisitor : IQuadtreeRangeVisitor<QTObstacleData>
        {
            public List<QTObstacleData> results;

            private Vector2Int _min;
            private Vector2Int _max;

            public bool OnVisit(QTObstacleData obj, AABB2D _, AABB2D __)
            {
                // Ensure position is within range
                if (_min.x <= obj.position.x && _min.y <= obj.position.y &&
                    _max.x >= obj.position.x && _max.y >= obj.position.y)
                    results.Add(obj);

                return true;
            }

            public QTRangeVisitor(Vector2Int min, Vector2Int max)
            {
                _min = min;
                _max = max;

                results = new List<QTObstacleData>();
            }
        }

        #endregion QT Structs


        /// <summary>
        /// Underlying QuadTree implementation
        /// </summary>
        private NativeQuadtree<QTObstacleData> _nativeQuadTree;

        private bool _quadTreeInitialized = false;

        /// <summary>
        /// Set of coordinates that have been inserted into the underlying quadtree,
        /// yet the associated obstacle has been removed.
        /// </summary>
        private HashSet<Vector2Int> _invalidPositions = new();

        /// <summary>
        /// Associated obstacle grid, will cross-reference obstacles from within
        /// for queries.
        /// </summary>
        private ISparseWorldGrid<Obstacle> _obstacleGrid;

        /// <summary>
        /// Obstacle flags used as a filter when refreshing the QuadTree
        /// </summary>
        private ObstacleFlags _obstacleFlags;


        /// <summary>
        /// Initializes an ObstacleQuadTree
        /// </summary>
        /// <param name="obstacleGrid">Associated obstacle grid</param>
        /// <param name="obstacleFlags">Obstacle flag filters</param>
        public ObstacleQuadTree(ISparseWorldGrid<Obstacle> obstacleGrid, ObstacleFlags obstacleFlags)
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
                new float2((float)obstaclePosition.x, (float)obstaclePosition.y)
            );
        }

        /// <summary>
        /// Removes given obstacle coordinate from valid coordinates in QuadTree.
        /// This coordinate can still be queried until the QuadTree is refreshed, 
        /// returning a null Obstacle.
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
            var visitor = new QTKNearestVisitor(k);
            float2 point = new((float)position.x, (float)position.y);
            _nativeQuadTree.Nearest(point, (float)maxDistance,
                ref visitor, default(QTManhattanDistanceProvider));

            List<Obstacle> queryResults = new(k);
            var invalidCount = 0;
            foreach (var data in visitor.kNearest)
            {
                // BLog.Log($"[ObstacleQuadTree] Queried obstacle at pos {data.position}. Checking if valid...");
                if (_invalidPositions.Contains(data.position))
                {
                    invalidCount++;

                    // Check if invalid threshold reached
                    if ((float)invalidCount / (float)k >= ObstacleConstants.QTMaxAllowedInvalidInQuery)
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

            var visitor = new QTRangeVisitor(min, max);

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
                if ((float)position.x < minPos.x) minPos = new float2((float)position.x, minPos.y);
                if ((float)position.y < minPos.y) minPos = new float2(minPos.x, (float)position.y);
                if ((float)position.x > maxPos.x) maxPos = new float2((float)position.x, maxPos.y);
                if ((float)position.y > maxPos.y) maxPos = new float2(maxPos.x, (float)position.y);
            }

            var boundsWidth = maxPos.x - minPos.x;
            var boundsHeight = maxPos.y - minPos.y;

            var horizontalPadding = boundsWidth * ObstacleConstants.QTBoundsPadding;
            var verticalPadding = boundsHeight * ObstacleConstants.QTBoundsPadding;

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
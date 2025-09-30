using NativeTrees;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Unity.Mathematics;
using Blizzard.Grid;
using System.Linq;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Wrapper for QuadTree storing obstacles
    /// </summary>
    public class ObstacleQuadTree
    {
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
        private struct QTreeKNearestVisitor : IQuadtreeNearestVisitor<QTObstacleData>
        {
            public List<QTObstacleData> kNearest;

            private int _k;

            public bool OnVist(QTObstacleData obj)
            {
                kNearest.Add(obj);

                // Continue iterating until k found
                return kNearest.Count < _k;
            }

            public QTreeKNearestVisitor(int k)
            {
                kNearest = new();

                _k = k;
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
        /// <param name="flags">Obstacle flag filters</param>
        public ObstacleQuadTree(ISparseWorldGrid<Obstacle> obstacleGrid, ObstacleFlags obstacleFlags)
        {
            this._obstacleGrid = obstacleGrid;
            this._obstacleFlags = obstacleFlags;
        }


        /// <summary>
        /// Adds given obstacle coordinate to QuadTree so it can be queried.
        /// </summary>
        public void Add(Vector2Int obstaclePosition)
        {
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

            Debug.Log($"[ObstacleQuadTree] Querying {k} nearest to {position}...");
            var visitor = new QTreeKNearestVisitor(k);
            float2 point = new((float)position.x, (float)position.y);
            _nativeQuadTree.Nearest(point, (float)maxDistance, 
                                    ref visitor, default(QTManhattanDistanceProvider));
            
            List<Obstacle> queryResults = new(k);
            int invalidCount = 0;
            foreach (QTObstacleData data in visitor.kNearest)
            {
                Debug.Log($"[ObstacleQuadTree] Queried obstacle at pos {data.position}. Checking if valid...");
                if (_invalidPositions.Contains(data.position)) 
                {
                    invalidCount++;

                    // Check if invalid threshold reached
                    if ((float)invalidCount / (float)k >= ObstacleConstants.QTmaxAllowedInvalidInQuery)
                    {
                        // Too many invalid retrieved in query! Rebuild and then restart query.
                        Rebuild();
                        return GetKNearestObstacles(position, k, maxDistance);
                    }
                    // Not reached, simply omit it.
                    continue;
                }

                Obstacle obstacle = _obstacleGrid.GetAt(data.position);
                Assert.IsTrue(obstacle != null, "QTObstacleData not marked invalid, yet obstacle" +
                    "doesn't exist in grid!");

                queryResults.Add(obstacle);
            }

            Debug.Log($"[ObstacleQuadTree] Successfully queried {queryResults.Count} obstacles!");
            return queryResults;
        }

        /// <summary>
        /// Rebuilds contents of the QuadTree, clearing all invalid entries.
        /// </summary>
        private void Rebuild()
        {
            Debug.Log("[ObstacleQuadTree] Rebuilding!");

            _invalidPositions.Clear();
            if (_obstacleGrid.Empty()) return; // Nothing in grid yet
            Vector2Int firstPos = _obstacleGrid.ValidPositions.First();
            float2 minPos = new(firstPos.x, firstPos.y);
            float2 maxPos = new(firstPos.x, firstPos.y);
            foreach (Vector2Int position in _obstacleGrid.ValidPositions)
            {
                if ((float)position.x < minPos.x) minPos = new((float)position.x, minPos.y);
                if ((float)position.y < minPos.y) minPos = new(minPos.x, (float)position.y);
                if ((float)position.x > maxPos.x) maxPos = new((float)position.x, maxPos.y);
                if ((float)position.y > maxPos.y) maxPos = new(maxPos.x, (float)position.y);
            }
            float boundsWidth = maxPos.x - minPos.x;
            float boundsHeight = maxPos.y - minPos.y;

            float horizontalPadding = boundsWidth * ObstacleConstants.QTBoundsPadding;
            float verticalPadding = boundsHeight * ObstacleConstants.QTBoundsPadding;

            minPos = new(minPos.x - horizontalPadding, minPos.y - verticalPadding);
            maxPos = new(maxPos.x + horizontalPadding, maxPos.y + verticalPadding);

            AABB2D bounds = new(minPos, maxPos);

            // Re-initialize quadtree
            _nativeQuadTree = new NativeQuadtree<QTObstacleData>(bounds, ObstacleConstants.QTobjectsPerNode,
                ObstacleConstants.QTmaxDepth, Unity.Collections.Allocator.Persistent, _obstacleGrid.Count);
            _quadTreeInitialized = true;
            foreach (var (position, obstacle) in _obstacleGrid)
            {
                if ((obstacle.ObstacleFlags & _obstacleFlags) == _obstacleFlags)
                {
                    // Filter met, add to quadtree
                    Add(position);
                }
            }
        }
    }
}
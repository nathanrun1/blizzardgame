using System.Collections;
using NativeTrees;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Blizzard.Grid;
using System.Linq;
using Blizzard.Constants;
using Blizzard.Utilities.Logging;
using Unity.Collections;

namespace Blizzard.Enemies.Core
{
    /// <summary>
    /// Wrapper for QuadTree storing obstacles
    /// </summary>
    public class EnemyQuadtree {
        #region QTStructs
        /// <summary>
        /// Represents a stored obstacle in a NativeQuadTree
        /// </summary>
        private struct QTEnemyData
        {
            /// <summary>
            /// Position of associated obstacle in obstacle grid
            /// </summary>
            public readonly Vector2 position;
            /// <summary>
            /// Index of enemy in enemy list to reference enemy instance
            /// </summary>
            public readonly int index;

            public QTEnemyData(Vector2 position, int index)
            {
                this.position = position;
                this.index = index;
            }
        }
        
        /// <summary>
        /// QuadTree Nearest Visitor that provides the K nearest elements
        /// </summary>
        private struct QTKNearestEnemyVisitor : IQuadtreeNearestVisitor<QTEnemyData>
        {
            public readonly List<EnemyBehaviour> kNearest;

            private readonly int _k;
            private readonly List<EnemyBehaviour> _enemies;
            private readonly HashSet<EnemyBehaviour> _inactiveEnemies;

            /// <summary>
            /// Amount of invalid queried elements so far
            /// </summary>
            private int _invalidCount;

            public bool OnVist(QTEnemyData obj)
            {
                if (_enemies[obj.index] && !_inactiveEnemies.Contains(_enemies[obj.index]))
                    kNearest.Add(_enemies[obj.index]);
                else 
                    _invalidCount++;
                
                // Continue iterating until k found or too many invalid
                return kNearest.Count < _k && _invalidCount < EnemyConstants.QTMaxNearestInvalid;
            }

            public QTKNearestEnemyVisitor(int k, List<EnemyBehaviour> enemies, HashSet<EnemyBehaviour> inactiveEnemies)
            {
                kNearest = new List<EnemyBehaviour>();

                _enemies = enemies;
                _inactiveEnemies = inactiveEnemies;
                _k = k;
                _invalidCount = 0;
            }
        }
        #endregion
        
        /// <summary>
        /// Underlying Quadtree A
        /// </summary>
        private NativeQuadtree<QTEnemyData> _quadtreeA = new(GameConstants.MapBounds, Allocator.Persistent);

        /// <summary>
        /// Underlying Quadtree B
        /// </summary>
        private NativeQuadtree<QTEnemyData> _quadtreeB = new(GameConstants.MapBounds, Allocator.Persistent);

        /// <summary>
        /// Whether quadtree A is the active quadtree.
        /// If false, indicates that quadtree B is the active quadtree.
        /// </summary>
        private bool _quadtreeAIsActive = true;

        /// <summary>
        /// Enemies that have been added
        /// </summary>
        private readonly List<EnemyBehaviour> _enemies = new();

        private int _curEnemyIndex = 0;
        /// <summary>
        /// Newly inactive enemies
        /// </summary>
        private readonly HashSet<EnemyBehaviour> _inactiveEnemies = new();

        /// <summary>
        /// Indices in the enemy list that were excluded in this cycle, to be removed from enemies in next cycle.
        /// </summary>
        private readonly SortedSet<int> _excludedIndices = new();
        
        public void Add(EnemyBehaviour enemy)
        {
            // Add to list of enemies, will be added to quadtree during this cycle
            _enemies.Add(enemy);
        }

        public void Remove(EnemyBehaviour enemy)
        {
            // Add to inactive set, will be removed at start of next cycle
            _inactiveEnemies.Add(enemy);
        }

        /// <summary>
        /// Per-tick update to the EnemyQuadtree.
        /// Each tick, a single enemy's position is updated in the quadtree.
        /// After all enemy positions have been updated in the inactive quadtree, the inactive quadtree is swapped in.
        /// </summary>
        public IEnumerator Tick()
        {
            while (true)
            {
                if (_enemies.Count == 0)
                {
                    // No enemies, nothing to update.
                    yield return null;
                    continue;
                }
                if (_curEnemyIndex >= _enemies.Count)
                {
                    // All enemies updated in inactive quadtree, swap!
                    _quadtreeAIsActive = !_quadtreeAIsActive;
                    // Reset newly inactive quadtree.
                    GetInactiveQuadtree() = new NativeQuadtree<QTEnemyData>(GameConstants.MapBounds, Allocator.Persistent);  
                    
                    // Reset index for new cycle
                    _curEnemyIndex = 0;
                    
                    // Clear excluded enemies from most recent cycle
                    // _excludedIndices is sorted, go in descending order to avoid invalidating later indices during removal
                    foreach (int excludedIndex in _excludedIndices.Reverse())
                    {
                        _enemies.RemoveAt(excludedIndex);
                    }
                    
                    // After swap wait until next tick (since _enemies has changed, index may be invalid)
                    yield return null;
                    continue;
                }
                
                EnemyBehaviour curEnemy = _enemies[_curEnemyIndex];
                if (curEnemy && !_inactiveEnemies.Contains(curEnemy))
                {
                    // Only insert currently valid enemies
                    Vector2 curEnemyPos = new(curEnemy.transform.position.x, curEnemy.transform.position.y);
                    int effectiveIndex = _curEnemyIndex - _excludedIndices.Count; // Effective index in next cycle, after removal of prev excluded
                    GetInactiveQuadtree().Insert(new QTEnemyData(curEnemyPos, effectiveIndex), new AABB2D(curEnemyPos, curEnemyPos));
                    yield return null;
                }
                else
                {
                    _excludedIndices.Add(_curEnemyIndex); // Exclude this index on next cycle
                    if (curEnemy)  // Was set to inactive, can remove from inactive set since has now been excluded.
                        _inactiveEnemies.Remove(curEnemy);
                }

                _curEnemyIndex++;
            }
        }

        /// <summary>
        /// Attempts to retrieve the k nearest enemies to the given position.
        /// May also retrieve any amount less than k, particularly when many nearby enemies
        /// have recently been invalidated (e.g. killed), or if there aren't enough enemies within
        /// the max distance.
        /// </summary>
        /// <param name="position">Reference position</param>
        /// <param name="k">Amount of nearest enemies to retrieve</param>
        /// <param name="maxDistance">Maximum distance of retrieved enemy</param>
        /// <returns>List of up to k nearest enemies, ordered nearest to farthest</returns>
        public List<EnemyBehaviour> GetKNearestEnemies(Vector2 position, int k, float maxDistance)
        {
            var visitor = new QTKNearestEnemyVisitor(k, _enemies, _inactiveEnemies);
            GetActiveQuadtree().Nearest(position, maxDistance, ref visitor, 
                new NativeQuadtreeExtensions.AABBDistanceSquaredProvider<QTEnemyData>());
            return visitor.kNearest;
        }
        
        /// <summary>
        /// Retrieves reference to the active quadtree
        /// </summary>
        private ref NativeQuadtree<QTEnemyData> GetActiveQuadtree()
        {
            if (_quadtreeAIsActive) return ref _quadtreeA;
            return ref _quadtreeB;
        }
        
        /// <summary>
        /// Retrieves reference to the inactive quadtree
        /// </summary>
        private ref NativeQuadtree<QTEnemyData> GetInactiveQuadtree()
        {
            if (_quadtreeAIsActive) return ref _quadtreeB;
            return ref _quadtreeA;
        }
    }
}
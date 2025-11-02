using System.Collections.Generic;
using NativeTrees;
using UnityEngine;

namespace Blizzard.Utilities.Quadtree
{
    public struct QTRangeVisitor<T> : IQuadtreeRangeVisitor<T>
    {
        public readonly List<T> results;

        private Vector2Int _min;
        private Vector2Int _max;

        public bool OnVisit(T obj, AABB2D objBounds, AABB2D queryRange)
        {
            // Ensure obj is within range
            if (objBounds.Overlaps(queryRange)) results.Add(obj);

            return true;
        }

        public QTRangeVisitor(Vector2Int min, Vector2Int max)
        {
            _min = min;
            _max = max;

            results = new List<T>();
        }
    }
}
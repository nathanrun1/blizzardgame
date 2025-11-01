using System.Collections.Generic;
using NativeTrees;

namespace Blizzard.Utilities.Quadtree
{
    /// <summary>
    /// QuadTree Nearest Visitor that provides the K nearest elements
    /// </summary>
    public readonly struct QTKNearestVisitor<T> : IQuadtreeNearestVisitor<T>
    {
        public readonly List<T> kNearest;

        private readonly int _k;

        public bool OnVist(T obj)
        {
            kNearest.Add(obj);

            // Continue iterating until k found
            return kNearest.Count < _k;
        }

        public QTKNearestVisitor(int k)
        {
            kNearest = new List<T>();

            _k = k;
        }
    }
}
using NativeTrees;
using Unity.Mathematics;
using UnityEngine;

namespace Blizzard.Utilities.Quadtree
{
    public struct QTManhattanDistanceProvider<T> : IQuadtreeDistanceProvider<T>
    {
        public float DistanceSquared(float2 point, T _, AABB2D bounds)
        {
            return Mathf.Abs(point.x - bounds.Center.x) +
                   Mathf.Abs(point.y - bounds.Center.y);
        }
    }
}
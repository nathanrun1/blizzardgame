using NativeTrees;
using UnityEngine;

namespace Blizzard.Utilities.DataTypes
{
    /// <summary>
    /// Defines a 2D bounding box in integer coordinates
    /// </summary>
    public struct AABBInt2D
    {
        /// <summary>
        /// Minimum coordinate of bounding box, inclusive
        /// </summary>
        public Vector2Int Min;
        
        /// <summary>
        /// Maximum coordinate of bounding box, inclusive
        /// </summary>
        public Vector2Int Max;

        public AABBInt2D(AABB2D aabb2D)
        {
            Min = new Vector2Int(Mathf.FloorToInt(aabb2D.min.x), Mathf.FloorToInt(aabb2D.min.y));
            Max = new Vector2Int(Mathf.CeilToInt(aabb2D.max.x), Mathf.CeilToInt(aabb2D.max.y));
        }
    }
}
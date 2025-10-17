using UnityEngine;

namespace Blizzard.Grid
{
    /// <summary>
    /// Describes a bounding box within a Grid
    /// </summary>
    public struct GridBounds
    {
        public Vector2Int offset;
        public Vector2Int size;
    }
}
using System.Collections.Generic;
using Blizzard.Utilities.DataTypes;
using UnityEngine;

namespace Blizzard.Utilities.Extensions
{
    public static class AABBInt2DExtensions
    {
        /// <summary>
        /// Enumerates all integer coordinates within the bounding box
        /// </summary>
        public static IEnumerable<Vector2Int> GetCoordinates(this AABBInt2D aabbInt2D)
        {
            for (int y = aabbInt2D.Min.y; y <= aabbInt2D.Max.y; ++y)
            {
                for (int x = aabbInt2D.Min.x; x <= aabbInt2D.Max.x; ++x)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }
    }
}
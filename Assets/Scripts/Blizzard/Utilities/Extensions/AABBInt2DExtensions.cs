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
        public static IEnumerable<Vector2Int> GetPoints(this AABBInt2D aabbInt2D)
        {
            for (int y = aabbInt2D.Min.y; y <= aabbInt2D.Max.y; ++y)
            {
                for (int x = aabbInt2D.Min.x; x <= aabbInt2D.Max.x; ++x)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }
        
        /// <summary>
        /// Generates a random point within the bounding box
        /// </summary>
        public static Vector2Int GetRandomPoint(this AABBInt2D aabbInt2D, System.Random rand)
        {
            return new Vector2Int(
                rand.Next(aabbInt2D.Min.x, aabbInt2D.Max.x + 1),
                rand.Next(aabbInt2D.Min.y, aabbInt2D.Max.y + 1)
            );
        }

        /// <summary>
        /// Determines whether the bounding box contains the given point
        /// </summary>
        public static bool Contains(this AABBInt2D aabbInt2D, Vector2Int point)
        {
            return
                aabbInt2D.Min.x <= point.x && point.x <= aabbInt2D.Max.x &&
                aabbInt2D.Min.y <= point.y && point.y <= aabbInt2D.Max.y;
        }
    }
}
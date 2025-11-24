using System.Collections.Generic;
using System.Linq;
using Blizzard.Utilities.Logging;
using Sirenix.Utilities;
using UnityEngine;

namespace Blizzard.Utilities.Assistants
{
    /// <summary>
    /// Helper class for grid-related operations
    /// </summary>
    public static class GridAssistant
    {
        /// <summary>
        /// Converts world position to grid position, given square grid cell side length
        /// </summary>
        public static Vector2Int WorldToCellPos(Vector2 worldPosition, float cellSideLength)
        {
            var gridPosition = new Vector2Int
            {
                x = Mathf.FloorToInt(worldPosition.x / cellSideLength),
                y = Mathf.FloorToInt(worldPosition.y / cellSideLength)
            };

            return gridPosition;
        }
        
        /// <summary>
        /// Converts grid position to world position of bottom left of grid square, given square grid cell side length
        /// </summary>
        public static Vector2 CellToWorldPosCorner(Vector2Int gridPosition, float cellSideLength)
        {
            Vector2 worldPosition;
            worldPosition.x = gridPosition.x * cellSideLength;
            worldPosition.y = gridPosition.y * cellSideLength;

            return worldPosition;
        }
        
        /// <summary>
        /// Converts grid position to world position of center of grid square, given square grid cell side length
        /// </summary>
        public static Vector2 CellToWorldPosCenter(Vector2Int gridPosition, float cellSideLength)
        {
            Vector2 worldPosition;
            worldPosition.x = gridPosition.x * cellSideLength + cellSideLength * 0.5f;
            worldPosition.y = gridPosition.y * cellSideLength + cellSideLength * 0.5f;

            return worldPosition;
        }

        /// <summary>
        /// Enumerates set of points within distance range [r1, r2] from p
        /// </summary>
        /// <param name="p">Reference point</param>
        /// <param name="r1">Minimum distance</param>
        /// <param name="r2">Maximum distance</param>
        public static IEnumerable<Vector2Int> GetPointsInDistanceRange(Vector2Int p, float r1, float r2)
        {
            int maxDistanceInt = Mathf.FloorToInt(r2);
            float r1_2 = r1 * r1;
            float r2_2 = r2 * r2;
            // Scan each valid y coordinate
            for (int y = -maxDistanceInt; y <= maxDistanceInt; ++y)
            {
                float y_2 = y * y;
                int xOuter = Mathf.FloorToInt(Mathf.Sqrt(r2_2 - y_2));  // Outer bound for x value
                if (y_2 >= r1_2)
                {
                    // No inner circle on this y value, return all coordinates within outer bound
                    IEnumerable<Vector2Int> withinOuter = Enumerable
                        .Range(-xOuter, 2 * xOuter + 1)
                        .Convert(x => new Vector2Int((int)x, y));
                    foreach (Vector2Int point in withinOuter)
                        yield return point + p;
                }
                else
                {
                    int xInner = Mathf.CeilToInt(Mathf.Sqrt(r1_2 - y_2));
                    // Inner circle present, add coordinates between inner/outer bounds
                    IEnumerable<Vector2Int> positiveX = Enumerable
                        .Range(xInner, (xOuter - xInner) + 1)
                        .Convert(x => new Vector2Int((int)x, y));
                    foreach (Vector2Int point in positiveX)
                        yield return point + p;
                    
                    // Mirror inner/outer bounds to account for negative x values
                    IEnumerable<Vector2Int> negativeX = Enumerable
                        .Range(xInner, (xOuter - xInner) + 1)
                        .Convert(x => new Vector2Int(-(int)x, y));
                    foreach (Vector2Int point in negativeX)
                        yield return point + p;
                }
            }
        }
    }
}
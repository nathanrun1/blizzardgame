using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Constants;
using Sirenix.Utilities;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Blizzard.Utilities.Assistants
{
    /// <summary>
    /// A rotation direction to follow when rotating around adjacent grid coordinates.
    /// </summary>
    public enum AdjRotation
    {
        Clockwise = -1,
        Counterclockwise = 1
    }
    
    /// <summary>
    /// Helper class for grid-related operations
    /// </summary>
    public static class GridAssistant
    {
        /// <summary>
        /// Ordered grid coordinates representing the rotation of adjacent grid coordinates, i.e.
        /// right -> top right -> top -> top left -> ...
        /// </summary>
        private static readonly Vector2Int[] _adjacentClock = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1)
        };

        /// <summary>
        /// Takes a grid coordinate representing an offset of at most distance sqrt(2) and a 'rot' value representing
        /// how many coordinates around the origin to rotate. Returns the resulting "rotation".
        ///
        /// E.g. (1,0) rotated by 2 yields (0,1)
        /// </summary>
        public static Vector2Int RotateAdjacentCoordinate(Vector2Int coordinate, AdjRotation rot)
        {
            int index = Array.IndexOf(_adjacentClock, coordinate);
            Assert.IsTrue(index != -1, $"Invalid coordinate to rotate: {coordinate}");

            index = (index + (int)rot) % 8;
            if (index < 0) index += 8;

            return _adjacentClock[index];
        }
        
        /// <summary>
        /// Converts world position to grid position, given square grid cell side length
        /// </summary>
        public static Vector2Int WorldToCellPos(Vector2 worldPosition, float cellSideLength = GameConstants.CellSideLength)
        {
            var gridPosition = new Vector2Int
            {
                x = Mathf.FloorToInt(worldPosition.x / cellSideLength),
                y = Mathf.FloorToInt(worldPosition.y / cellSideLength)
            };

            return gridPosition;
        }

        /// <summary>
        /// Converts world position to grid position, given square grid cell side length
        /// </summary>
        public static Vector2Int ToCellPos(this Vector2 worldPosition, float cellSideLength = GameConstants.CellSideLength)
        {
            return GridAssistant.WorldToCellPos(worldPosition, cellSideLength);
        }
        
        /// <summary>
        /// Converts grid position to world position of bottom left of grid square, given square grid cell side length
        /// </summary>
        public static Vector2 CellToWorldPosCorner(Vector2Int gridPosition, float cellSideLength = GameConstants.CellSideLength)
        {
            Vector2 worldPosition;
            worldPosition.x = gridPosition.x * cellSideLength;
            worldPosition.y = gridPosition.y * cellSideLength;

            return worldPosition;
        }
        
        /// <summary>
        /// Converts grid position to world position of center of grid square, given square grid cell side length
        /// </summary>
        public static Vector2 CellToWorldPosCenter(Vector2Int gridPosition, float cellSideLength = GameConstants.CellSideLength)
        {
            Vector2 worldPosition;
            worldPosition.x = gridPosition.x * cellSideLength + cellSideLength * 0.5f;
            worldPosition.y = gridPosition.y * cellSideLength + cellSideLength * 0.5f;

            return worldPosition;
        }

        public static Vector2 ToWorldPosCenter(this Vector2Int gridPosition, float cellSideLength = GameConstants.CellSideLength)
        {
            return GridAssistant.CellToWorldPosCenter(gridPosition, cellSideLength);
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
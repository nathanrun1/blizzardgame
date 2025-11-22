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
    }
}
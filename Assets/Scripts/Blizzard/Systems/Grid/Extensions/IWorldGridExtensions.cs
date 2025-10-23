using UnityEngine;

namespace Blizzard.Grid
{
    public static class IWorldGridExtensions
    {
        /// <summary>
        /// Converts world position to grid position
        /// </summary>
        public static Vector2Int WorldToCellPos<T>(this IWorldGrid<T> grid, Vector2 worldPosition)
        {
            var gridPosition = new Vector2Int();
            gridPosition.x = Mathf.FloorToInt(worldPosition.x / grid.CellWidth);
            gridPosition.y = Mathf.FloorToInt(worldPosition.y / grid.CellHeight);

            return gridPosition;
        }

        /// <summary>
        /// Converts grid position to world position of bottom left of grid square
        /// </summary>
        public static Vector2 CellToWorldPosCorner<T>(this IWorldGrid<T> grid, Vector2Int gridPosition)
        {
            Vector2 worldPosition;
            worldPosition.x = gridPosition.x * grid.CellWidth;
            worldPosition.y = gridPosition.y * grid.CellHeight;

            return worldPosition;
        }

        /// <summary>
        /// Converts grid position to world position of center of grid square
        /// </summary>
        public static Vector2 CellToWorldPosCenter<T>(this IWorldGrid<T> grid, Vector2Int gridPosition)
        {
            Vector2 worldPosition;
            worldPosition.x = gridPosition.x * grid.CellWidth + grid.CellWidth * 0.5f;
            worldPosition.y = gridPosition.y * grid.CellHeight + grid.CellHeight * 0.5f;

            return worldPosition;
        }
    }
}
using System;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Blizzard.Grid
{
    public static class IGridExtensions
    {
        /// <summary>
        /// Writes to dense grid from area defined by dense grid size and offset
        /// </summary>
        public static void WriteToDenseGrid<T>(this IGrid<T> grid, IDenseGrid<T> denseGrid, Vector2Int offset)
        {
            for (int y = offset.y; y < offset.y + denseGrid.Height; ++y)
            {
                for (int x = offset.x; x < offset.x + denseGrid.Width; ++x)
                {
                    int subgridIndex = (y - offset.y) * denseGrid.Width + (x - offset.x);
                    denseGrid.SetAt(x, y, grid.GetAt(x, y));
                }
            }
        }

        /// <summary>
        /// Reads given dense grid into this grid in area defined by dense grid size and offset
        /// </summary>
        public static void ReadFromDenseGrid<T>(this IGrid<T> grid, IDenseGrid<T> denseGrid, Vector2Int offset)
        {
            for (int y = offset.y; y < offset.y + denseGrid.Height; ++y)
            {
                for (int x = offset.x; x < offset.x + denseGrid.Width; ++x)
                {
                    int subgridIndex = (y - offset.y) * denseGrid.Width + (x - offset.x);
                    grid.SetAt(x, y, denseGrid.GetAt(x, y));
                }
            }
        }
    }
}

// "Just do it."

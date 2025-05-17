using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.Grid
{
    /// <summary>
    /// A 2D grid of cells, each cell containing data of type T
    /// </summary>
    public interface IGrid<T>
    {
        /// <summary>
        /// Gets cell data at given grid position
        /// </summary>
        public T GetAt(int x, int y);

        /// <summary>
        /// Gets cell data at given grid position
        /// </summary>
        public T GetAt(Vector2Int gridPosition);

        /// <summary>
        /// Sets value to cell data at given grid position if it exists and returns true, otherwise returns false and sets value to null.
        /// </summary>
        public bool TryGetValue(int x, int y, out T value);

        /// <summary>
        /// Sets value to cell data at given grid position if it exists and returns true, otherwise returns false and sets value to null.
        /// </summary>
        public bool TryGetValue(Vector2Int gridPosition, out T value);

        /// <summary>
        /// Sets cell data at given grid position to given value
        /// </summary>
        public void SetAt(int x, int y, T value);

        /// <summary>
        /// Sets cell data at given grid position to given value
        /// </summary>
        public void SetAt(Vector2Int gridPosition, T value);

        /// <summary>
        /// Sets cell data at given grid position to default value
        /// </summary>
        public void RemoveAt(Vector2Int gridPosition);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Blizzard.Grid
{
    /// <summary>
    /// A 2D grid of cells of arbitrary size, each cell containing data of type T
    /// </summary>
    public interface ISparseGrid<T> : IGrid<T>, IEnumerable<KeyValuePair<Vector2Int, T>>
    {
        /// <summary>
        /// Default value of a cell
        /// </summary>
        public T DefaultCell { get; set; }
    }
}
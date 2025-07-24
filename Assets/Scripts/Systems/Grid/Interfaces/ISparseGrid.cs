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

        /// <summary>
        /// Valid grid positions, i.e. grid positions containing non-default values
        /// </summary>
        public IEnumerable<Vector2Int> ValidPositions { get; }

        /// <summary>
        /// All values stored in the sparse grid
        /// </summary>
        public IEnumerable<T> Values { get; }
    }
}
using System.Collections.Generic;

namespace Blizzard.Grid
{
    /// <summary>
    /// A 2D grid of cells of fixed size, each cell containing data of type T
    /// </summary>
    public interface IDenseGrid<T> : IGrid<T>, IEnumerable<T>
    {
        /// <summary>
        /// Amount of rows in the grid
        /// </summary>
        public abstract int Height { get; }
        /// <summary>
        /// Amount of columns in the grid
        /// </summary>
        public abstract int Width { get; }

        /// <summary>
        /// Initializes all cell data in grid to default value of T
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Initializes all cell data in grid to given value
        /// </summary>
        public abstract void Initialize(T value);

        /// <summary>
        /// Returns raw & flattened data contained within the grid
        /// </summary>
        public T[] GetData();
    }
}

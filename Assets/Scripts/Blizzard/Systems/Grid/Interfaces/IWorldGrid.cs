namespace Blizzard.Grid
{
    /// <summary>
    /// A 2D fixed size grid of cells representing area in the scene. Each cell contains data of type T.
    /// </summary>
    public interface IWorldGrid<T> : IGrid<T>
    {
        /// <summary>
        /// Height of the rectangular area in the scene that a cell represents
        /// </summary>
        public abstract float CellHeight { get; }

        /// <summary>
        /// Width of the rectangular area in the scene that a cell represents
        /// </summary>
        public abstract float CellWidth { get; }
    }
}
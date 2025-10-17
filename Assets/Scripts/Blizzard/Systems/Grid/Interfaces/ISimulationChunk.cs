namespace Blizzard.Grid
{
    public enum NeighborLocation
    {
        Above, Below, Left, Right
    }

    /// <summary>
    /// A dense simulated chunk, meant to represent a subregion of a simulated grid.
    /// </summary>
    public interface ISimulationChunk<T> : IDenseGrid<T>, ISimulationGrid<T>
    {
        /// <summary>
        /// Default cell data as placeholder for cell locations with no stored data
        /// </summary>
        public T DefaultCell { get; set; }
    }
}
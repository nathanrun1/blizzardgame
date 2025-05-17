namespace Blizzard.Grid
{
    /// <summary>
    /// A 2D grid of cells. Each cell contains data of type T.
    /// The grid has a defined simulation step method that updates the grid data based on a given time delta.
    /// </summary>
    public interface ISimulationGrid<T> : IGrid<T>
    {
        /// <summary>
        /// Updates the grid data given a time delta.
        /// </summary>
        public abstract void Step(float deltaTime);
    }
}
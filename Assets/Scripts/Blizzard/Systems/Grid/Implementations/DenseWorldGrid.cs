namespace Blizzard.Grid
{
    public class DenseWorldGrid<T> : BasicDenseGrid<T>, IWorldGrid<T>
    {
        public float CellHeight { get; private set; }

        public float CellWidth { get; private set; }

        public DenseWorldGrid(float cellHeight, float cellWidth, int width, int height, T[] data = null)
            : base(width, height, data)
        {
            CellHeight = cellHeight;
            CellWidth = cellWidth;
        }
    }
}
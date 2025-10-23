using UnityEngine;

namespace Blizzard.Utilities
{
    /// <summary>
    /// Tracks the outermost bounds of a set of Vector2Int positions
    /// </summary>
    public class OuterMostBounds
    {
        public Vector2Int MaxBound => new(_xBounds.GetMax(), _yBounds.GetMax());
        public Vector2Int MinBound => new(_xBounds.GetMin(), _yBounds.GetMin());

        private MinMaxHeap<int> _xBounds = new();
        private MinMaxHeap<int> _yBounds = new();

        /// <summary>
        /// Add position to set
        /// </summary>
        /// <param name="position"></param>
        public void Add(Vector2Int position)
        {
            _xBounds.Add(position.x);
            _yBounds.Add(position.y);
        }

        /// <summary>
        /// Remove position from set
        /// </summary>
        /// <param name="position"></param>
        public void Remove(Vector2Int position)
        {
            _xBounds.Remove(position.x);
            _yBounds.Remove(position.y);
        }

        /// <summary>
        /// Whether OuterMostBounds is empty
        /// </summary>
        public bool IsEmpty()
        {
            return _xBounds.IsEmpty() || _yBounds.IsEmpty();
        }
    }
}
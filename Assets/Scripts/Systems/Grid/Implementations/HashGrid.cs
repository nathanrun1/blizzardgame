using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.Grid
{
    /// <summary>
    /// Implementation of grid using hash table.
    /// </summary>
    public class HashGrid<T> : ISparseGrid<T>
    {
        public T DefaultCell { get; set; }

        public IEnumerable<Vector2Int> ValidPositions { get => _hashmap.Keys; }

        public IEnumerable<T> Values { get => _hashmap.Values;  }

        private Dictionary<Vector2Int, T> _hashmap = new Dictionary<Vector2Int, T>();



        public T GetAt(int x, int y)
        {
            return GetAt(new(x, y));
        }

        public T GetAt(Vector2Int gridPosition)
        {
            return _hashmap[gridPosition];
        }

        public bool TryGetValue(int x, int y, out T value)
        {
            return TryGetValue(new(x, y), out value);
        }

        public bool TryGetValue(Vector2Int gridPosition, out T value)
        {
            return _hashmap.TryGetValue(gridPosition, out value);
        }

        public void SetAt(int x, int y, T value)
        {
            SetAt(new(x, y), value);
        }

        public void SetAt(Vector2Int gridPosition, T value)
        {
            _hashmap[gridPosition] = value;
        }

        public void ResetAt(Vector2Int gridPosition)
        {
            _hashmap.Remove(gridPosition);
        }

        public IEnumerator<KeyValuePair<Vector2Int, T>> GetEnumerator()
        {
            return _hashmap.GetEnumerator() as IEnumerator<KeyValuePair<Vector2Int, T>>;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
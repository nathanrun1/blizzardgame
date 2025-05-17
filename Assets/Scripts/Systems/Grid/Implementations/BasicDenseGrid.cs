using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blizzard.Grid
{
    public class BasicDenseGrid<T> : IDenseGrid<T>
    {
        public virtual int Width { get => _width; protected set => _width = value; }
        protected int _width;

        public virtual int Height { get => _width; protected set => _width = value; }
        protected int _height;

        /// <summary>
        /// Raw grid data
        /// </summary>
        private T[] _data;

        /// <summary>
        /// Creates uninitialized grid with given dimensions, and optionally given flattened raw data.
        /// Assumes raw data respects given dimensions.
        /// </summary>
        public BasicDenseGrid(int width, int height, T[] data = null)
        {
            if (data == null) _data = new T[width * height];
            else _data = data;
            _width = width;
            _height = height;
        }

        public void Initialize()
        {
            _data.Initialize();
        }

        public void Initialize(T value)
        {
            for (int y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    _data[GetFlattenedIndex(x, y)] = value;
                }
            }
        }

        public virtual T GetAt(int x, int y)
        {
            ValidateGridPosition(x, y);
            return _data[GetFlattenedIndex(x, y)];
        }

        public T GetAt(Vector2Int gridPosition)
        {
            return GetAt(gridPosition.x, gridPosition.y);
        }

        public virtual bool TryGetValue(int x, int y, out T value)
        {
            if (!IsGridPositionValid(x, y))
            {
                value = default(T);
                return false;
            }
            else
            {
                value = GetAt(x, y);
                return true;
            }
        }

        public bool TryGetValue(Vector2Int gridPosition, out T value)
        {
            return TryGetValue(gridPosition.x, gridPosition.y, out value);
        }

        public virtual void SetAt(int x, int y, T value)
        {
            ValidateGridPosition(x, y);
            _data[GetFlattenedIndex(x, y)] = value;
        }

        public void SetAt(Vector2Int gridPosition, T value)
        {
            SetAt(gridPosition.x, gridPosition.y, value);
        }

        public void RemoveAt(Vector2Int gridPosition)
        {
            SetAt(gridPosition.x, gridPosition.y, default);
        }

        public T[] GetData()
        {
            return _data;
        }

        /// <summary>
        /// Checks if given grid position is out of range, throws exception if so.
        /// </summary>
        private void ValidateGridPosition(int x, int y)
        {
            if (!IsGridPositionValid(x, y))
            {
                throw new ArgumentOutOfRangeException($"Grid position ({x}, {y}) out of range!");
            }
        }

        /// <summary>
        /// Returns true if given grid position is in range, false if not.
        /// </summary>
        private bool IsGridPositionValid(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return false;
            return true;
        }

        protected int GetFlattenedIndex(int x, int y)
        {
            return y * _width + x;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator() as IEnumerator<T>;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}


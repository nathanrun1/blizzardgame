using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Blizzard.Grid
{
    public class HashWorldGrid<T> : HashGrid<T>, ISparseWorldGrid<T>
    {
        public float CellHeight { get; protected set; }
        public float CellWidth { get; protected set; }

        public HashWorldGrid(float cellHeight, float cellWidth)
            : base()
        {
            CellHeight = cellHeight;
            CellWidth = cellWidth;
        }
    }
}
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Blizzard.Grid
{
    public class FullHashWorldGrid<T> : FullHashGrid<T>, ISparseWorldGrid<T>
    {
        public float CellHeight { get; protected set; }
        public float CellWidth { get; protected set; }

        public FullHashWorldGrid(float cellHeight, float cellWidth)
            : base()
        {
            this.CellHeight = cellHeight;
            this.CellWidth = cellWidth;
        }
    }
}
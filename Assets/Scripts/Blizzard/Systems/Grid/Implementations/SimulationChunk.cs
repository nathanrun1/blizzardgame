using Blizzard.Temperature;
using System;
using Blizzard.Constants;
using UnityEngine;
using Zenject;

namespace Blizzard.Grid
{
    public class SimulationChunk<T> : BasicDenseGrid<T>, ISimulationChunk<T>
    {
        private const int PADDING = 1;


        // We separate interface dimensions from the dimensions of underlying dense grid, since we use 1 cell wide padding in the underlying grid
        // that is part of the implementation only
        public override int Width => _simulatedWidth;
        public override int Height => _simulatedHeight;

        public T DefaultCell { get; set; }

        /// <summary>
        /// Width of public data
        /// </summary>
        protected int _simulatedWidth;

        /// <summary>
        /// Height of public data
        /// </summary>
        protected int _simulatedHeight;

        private T[] _storedNeighbors = new T[4];


        [Inject] private ComputeShader _heatDiffusionShader;

        private ComputeBuffer _inputBuffer;
        private ComputeBuffer _outputBuffer;


        public SimulationChunk(int width, int height, T defaultCell) :
            base(width + 2, height + 2, null) // add 2 to underlying width & height for neighboring cells
        {
            _simulatedWidth = width;
            _simulatedHeight = height;

            Initialize(DefaultCell);
        }

        /// <summary>
        /// Sets column/row containing neighboring cells of neighboring chunk based on given neighbor location.
        /// If no neighbor chunk provided, sets data instead to current default cell data.
        /// 
        /// Neighbor data must be set before every simulation step, old data will be replaced with default
        /// </summary>
        public void SetNeighborData(NeighborLocation location, ISimulationChunk<T> neighbor = null)
        {
            switch (location)
            {
                case NeighborLocation.Left:
                {
                    var localX = 0; // left padding column
                    var neighborX = neighbor.Width - 1; // rightmost column of neighbor
                    if (neighbor != null)
                        for (var y = 0; y < _simulatedHeight; ++y)
                            base.SetAt(localX, y + PADDING, neighbor.GetAt(neighborX, y));
                    else
                        for (var y = 0; y < _simulatedHeight; ++y)
                            base.SetAt(localX, y + PADDING, DefaultCell);

                    break;
                }
                case NeighborLocation.Right:
                {
                    var localX = _width - 1; // right padding column
                    var neighborX = 0; // leftmost column of neighbor
                    if (neighbor != null)
                        for (var y = 0; y < _simulatedHeight; ++y)
                            base.SetAt(localX, y + PADDING, neighbor.GetAt(neighborX, y));
                    else
                        for (var y = 0; y < _simulatedHeight; ++y)
                            base.SetAt(localX, y + PADDING, DefaultCell);

                    break;
                }
                case NeighborLocation.Above:
                {
                    var localY = _height - 1; // top padding row
                    var neighborY = 0; // bottom row of neighbor
                    if (neighbor != null)
                    {
                        for (var x = 0; x < _height; ++x) base.SetAt(x + PADDING, localY, neighbor.GetAt(x, neighborY));
                        break;
                    }
                    else
                    {
                        for (var x = 0; x < _height; ++x) base.SetAt(x + PADDING, localY, DefaultCell);
                        break;
                    }
                }
                case NeighborLocation.Below:
                {
                    var localY = 0; // bottom padding row
                    var neighborY = neighbor.Height - 1; // top row of neighbor
                    if (neighbor != null)
                    {
                        for (var x = 0; x < _height; ++x) base.SetAt(x + PADDING, localY, neighbor.GetAt(x, neighborY));
                        break;
                    }
                    else
                    {
                        for (var x = 0; x < _height; ++x) base.SetAt(x + PADDING, localY, DefaultCell);
                        break;
                    }
                }
            }
        }

        public void Step(float deltaTime)
        {
            _heatDiffusionShader.SetFloat("deltaTime", deltaTime);

            _inputBuffer.SetData(GetData());

            var threadGroupSize =
                TemperatureConstants.ComputeThreadGroupDimensions.x *
                TemperatureConstants.ComputeThreadGroupDimensions.y;

            _heatDiffusionShader.Dispatch(0, _width * _height / threadGroupSize, 1, 1);
            var outputData = new T[_width * _height];
            _outputBuffer.GetData(outputData); // TODO: async this (currently waits for GPU to finish)

            // Note: double copy here, copies first into temporary data array, and then again into chunk's data array

            // Only copy data not in padding (i.e. don't touch the cells copied from neighboring chunks)
            for (var x = PADDING; x < _width - PADDING; ++x)
            for (var y = PADDING; y < _height - PADDING; ++y)
                base.SetAt(x, y, outputData[GetFlattenedIndex(x, y)]);
        }

        public override T GetAt(int x, int y)
        {
            ValidateGridPosition(x, y);
            return base.GetAt(x + PADDING, y + PADDING);
        }

        public override bool TryGetValue(int x, int y, out T value)
        {
            if (!IsGridPositionValid(x, y))
            {
                value = default;
                return false;
            }

            return base.TryGetValue(x + PADDING, y + PADDING, out value);
        }

        public override void SetAt(int x, int y, T value)
        {
            ValidateGridPosition(x, y);
            base.SetAt(x + PADDING, y + PADDING, value);
        }


        /// <summary>
        /// Sets all neighbor cells to the current default cell, intended to be invoked after every simulation step
        /// </summary>
        private void ResetNeighborData()
        {
            SetNeighborData(NeighborLocation.Left, null);
            SetNeighborData(NeighborLocation.Right, null);
            SetNeighborData(NeighborLocation.Above, null);
            SetNeighborData(NeighborLocation.Below, null);
        }

        private void SetupComputeShader()
        {
            _inputBuffer = new ComputeBuffer(_width * _height, TemperatureCell.GetSize());
            _outputBuffer = new ComputeBuffer(_width * _height, TemperatureCell.GetSize());

            _heatDiffusionShader.SetBuffer(0, "prev", _inputBuffer);
            _heatDiffusionShader.SetBuffer(0, "next", _outputBuffer);
            _heatDiffusionShader.SetInt("width", _width);
            _heatDiffusionShader.SetInt("height", _height);
            _heatDiffusionShader.SetInt("selectedX", 0); // temp
            _heatDiffusionShader.SetInt("selectedY", 0); // temp
            _heatDiffusionShader.SetFloat("diffusionFactor", TemperatureConstants.DiffusionFactor);
        }

        private void DisposeComputeBuffers()
        {
            _inputBuffer.Dispose();
            _outputBuffer.Dispose();
        }

        /// <summary>
        /// Checks if given grid position is out of range, throws exception if so.
        /// </summary>
        private void ValidateGridPosition(int x, int y)
        {
            if (!IsGridPositionValid(x, y))
                throw new ArgumentOutOfRangeException($"Grid position ({x}, {y}) out of range!");
        }

        /// <summary>
        /// Returns true if given grid position is in range, false if not.
        /// </summary>
        private bool IsGridPositionValid(int x, int y)
        {
            if (x < 0 || x >= _simulatedWidth || y < 0 || y >= _simulatedHeight) return false;
            return true;
        }
    }
}
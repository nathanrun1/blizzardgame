using UnityEngine;
using Blizzard.Grid;
using System.Collections;
using UnityEditor.PackageManager.UI;
using System;


namespace Blizzard.Temperature
{
    public class TemperatureService
    {
        /// <summary>
        /// Invoked when a temperature simulation step is completed
        /// </summary>
        public event Action OnTemperatureUpdate;

        /// <summary>
        /// Grid containing temperature data for a large set region of the scene
        /// </summary>
        public IWorldGrid<TemperatureCell> Grid { get; private set; }
        /// <summary>
        /// Texture displaying a heatmap of the current computed subregion of the scene.
        /// </summary>
        public RenderTexture HeatmapTexture { get; private set; }

        /// <summary>
        /// Subregion of grid that gets updated at each step
        /// </summary>
        private IDenseGrid<TemperatureCell> _window;
        /// <summary>
        /// Offset of window location in main grid
        /// </summary>
        private Vector2Int _windowOffset;

        /// <summary>
        /// Compute shader used to compute heat diffusion
        /// </summary>
        private ComputeShader _heatDiffusionShader;
        /// <summary>
        /// Stores temperature data copied from window to compute shader
        /// </summary>
        private ComputeBuffer _inputBuffer;
        /// <summary>
        /// Stores temperature data copied from compute shader to window
        /// </summary>
        private ComputeBuffer _outputBuffer;

        public TemperatureService(IWorldGrid<TemperatureCell> grid, IDenseGrid<TemperatureCell> window, ComputeShader heatDiffusionShader)
        {
            Grid = grid;
            _window = window;
            _heatDiffusionShader = heatDiffusionShader;
            SetupComputeShader();
            SetupHeatmap();
        }

        ~TemperatureService()
        {
            DisposeComputeBuffers();
        }

        /// <summary>
        /// Compute a single heat diffusion step using given delta time. Updates temperature grid and heatmap texture.
        /// </summary>
        public void DoHeatDiffusionStep(float deltaTime)
        {
            UpdateActiveSubgrid(new(0, 0)); // TODO: get offset from somewhere
            _heatDiffusionShader.SetFloat("deltaTime", deltaTime);

            int threadGroupSize =
                TemperatureConstants.ComputeThreadGroupDimensions.x *
                TemperatureConstants.ComputeThreadGroupDimensions.y;
            _heatDiffusionShader.Dispatch(0, (_window.Width * _window.Height) / threadGroupSize, 1, 1);
            _outputBuffer.GetData(_window.GetData()); // TODO: async this (currently waits for GPU to finish)
            Grid.ReadFromDenseGrid(_window, new(0, 0)); // TODO: get offset from somewhere

            OnTemperatureUpdate?.Invoke();
        }

        public void ComputeHeatmap()
        {
            _heatDiffusionShader.Dispatch(1, _window.Width, _window.Height, 1);
        }





        /// <summary>
        /// Populates temperature grid with random temperatures between given range
        /// </summary>
        //private void PopulateRandomTemperatureData(float minInclusive, float maxInclusive)
        //{
        //    for (int x = 0; x < Grid.Width; ++x)
        //    {
        //        for (int y = 0; y < Grid.Height; ++y)
        //        {
        //            TemperatureCell cur = Grid.GetAt(x, y);
        //            Grid.SetAt(x, y, new TemperatureCell
        //            {
        //                temperature = Random.Range(minInclusive, maxInclusive),
        //                insulation = cur.insulation,
        //                heat = cur.heat
        //            });
        //        }
        //    }
        //}

        /// <summary>
        /// Fetches data from main grid into window grid, and copies it to input compute buffer.
        /// </summary>
        private void UpdateActiveSubgrid(Vector2Int offset)
        {
            Grid.WriteToDenseGrid(_window, offset);
            _inputBuffer.SetData(_window.GetData());
        }
        
        /// <summary>
        /// Sets paramaters for the compute shader.
        /// </summary>
        private void SetupComputeShader()
        {
            // Initialize compute buffers
            _inputBuffer = new ComputeBuffer(_window.Width * _window.Height, TemperatureCell.GetSize());
            _outputBuffer = new ComputeBuffer(_window.Width * _window.Height, TemperatureCell.GetSize());

            _heatDiffusionShader.SetBuffer(0, "prev", _inputBuffer);
            _heatDiffusionShader.SetBuffer(0, "next", _outputBuffer);
            _heatDiffusionShader.SetInt("width", _window.Width);
            _heatDiffusionShader.SetInt("height", _window.Height);
            _heatDiffusionShader.SetFloat("diffusionFactor", TemperatureConstants.DiffusionFactor);
        }

        /// <summary>
        /// Initializes heatmap texture and sets associated parameters
        /// </summary>
        private void SetupHeatmap()
        {
            // Setup heatmap texture
            HeatmapTexture = new RenderTexture(_window.Width * 8, _window.Height * 8, 1);
            HeatmapTexture.enableRandomWrite = true;
            HeatmapTexture.format = RenderTextureFormat.ARGBFloat;
            HeatmapTexture.Create();

            _heatDiffusionShader.SetBuffer(1, "prev", _inputBuffer);
            _heatDiffusionShader.SetTexture(1, "heatmap", HeatmapTexture);
            _heatDiffusionShader.SetFloat("defaultAlpha", 1);
            _heatDiffusionShader.SetInt("selectedX", 0);
            _heatDiffusionShader.SetInt("selectedY", 0);
        }

        private void DisposeComputeBuffers()
        {
            _inputBuffer.Dispose();
            _outputBuffer.Dispose();
        }
    }
}

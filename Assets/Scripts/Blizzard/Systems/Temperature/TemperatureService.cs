using UnityEngine;
using Blizzard.Grid;
using System;
using Blizzard.Constants;
using Zenject;
using ModestTree;


namespace Blizzard.Temperature
{
    public class TemperatureService : IFixedTickable
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
        /// Offset of the updated active subgrid
        /// </summary>
        public Vector2Int WindowOffset { get; set; }
        /// <summary>
        /// Whether temperature simulation is active
        /// </summary>
        public bool Active { get; set; }

        public float AmbientTemperature
        {
            get => _ambientTemperature;
            set
            {
                // Update compute shader
                _heatDiffusionShader.SetFloat("ambientTemperature", value);
                _ambientTemperature = value;
            }
        }
        private float _ambientTemperature;


        /// <summary>
        /// Subregion of grid that gets updated at each step
        /// </summary>
        private IDenseGrid<TemperatureCell> _window;

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


        public TemperatureService(IWorldGrid<TemperatureCell> grid, 
                                  IDenseGrid<TemperatureCell> window, 
                                  ComputeShader heatDiffusionShader,
                                  bool isActive = true)
        {
            Grid = grid;
            _window = window;
            _heatDiffusionShader = heatDiffusionShader;
            Active = isActive;
            SetupComputeShader();
            SetupHeatmap();
        }

        ~TemperatureService()
        {
            DisposeComputeBuffers();
        }

        public void FixedTick()
        {
            if (!Active) return;
            DoHeatDiffusionStep(Time.fixedDeltaTime, WindowOffset);
            ComputeHeatmap();
        }

        /// <summary>
        /// Compute a single heat diffusion step using given delta time. Updates temperature grid and heatmap texture.
        /// </summary>
        public void DoHeatDiffusionStep(float deltaTime, Vector2Int offset)
        {
            UpdateActiveSubgrid(offset); 
            _heatDiffusionShader.SetFloat("deltaTime", deltaTime);

            int threadGroupSize =
                TemperatureConstants.ComputeThreadGroupDimensions.x *
                TemperatureConstants.ComputeThreadGroupDimensions.y;
            _heatDiffusionShader.Dispatch(0, (_window.Width * _window.Height) / threadGroupSize, 1, 1);
            _outputBuffer.GetData(_window.GetData()); // TODO: async this (currently waits for GPU to finish)
            Grid.ReadFromDenseGrid(_window, offset);

            OnTemperatureUpdate?.Invoke();
        }

        /// <summary>
        /// Computes current heatmap from most recent diffusion step
        /// </summary>
        public void ComputeHeatmap()
        {
            _heatDiffusionShader.Dispatch(1, _window.Width, _window.Height, 1);
        }

        /// <summary>
        /// Retrieves temperature value at given world position
        /// </summary>
        public float GetTemperatureAtWorldPos(Vector2 worldPosition)
        {
            Vector2Int gridPos = Grid.WorldToCellPos(worldPosition);
            return Grid.GetAt(gridPos).temperature;
        }

        /// <summary>
        /// Retrieves temperature value at given world position, translating from 3D to 2D world position
        /// </summary>
        public float GetTemperatureAtWorldPos(Vector3 worldPosition3d)
        {
            Vector2 worldPosition = new(worldPosition3d.x, worldPosition3d.y);
            return GetTemperatureAtWorldPos(worldPosition);
        }

        /// <summary>
        /// Sets compute shader float value, intended only for testing purposes
        /// </summary>
        public void SetComputeFloat(string nameId, float value)
        {
            Assert.That(Application.isEditor);
            _heatDiffusionShader.SetFloat(nameId, value);
        }

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

            _heatDiffusionShader.SetFloat("ambientTemperature", TemperatureConstants.StartingAmbientTemperature);
            _heatDiffusionShader.SetFloat("ambientFactor", TemperatureConstants.AmbientFactor);
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

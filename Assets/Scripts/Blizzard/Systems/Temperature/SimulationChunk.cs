using Blizzard.Grid;
using System.Numerics;
using UnityEngine;

namespace Blizzard.Temperature
{
    public class SimulationChunk
    {
        /// <summary>
        /// Main grid containing all temperature data, this chunk defines a subset location of this grid
        /// </summary>
        private IWorldGrid<TemperatureCell> _grid;

        /// <summary>
        /// Compute shader used to compute heat diffusion
        /// </summary>
        private ComputeShader _heatDiffusionShader;

        public SimulationChunk(IWorldGrid<TemperatureCell> grid, Vector2Int offset, ComputeShader heatDiffusionShader)
        {
        }
    }
}
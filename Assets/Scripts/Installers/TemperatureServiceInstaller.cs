using UnityEngine;
using Zenject;
using Blizzard.Grid;

namespace Blizzard.Temperature
{
    public class TemperatureServiceInstaller : MonoInstaller
    {
        const int SIM_WINDOW_WIDTH = 32;
        const int SIM_WINDOW_HEIGHT = 32;
        const float CELL_SIDE_LENGTH = 0.5f;

        [SerializeField] ComputeShader _heatDiffusionShader;

        public override void InstallBindings()
        {
            var mainGrid = new DenseWorldGrid<TemperatureCell>(CELL_SIDE_LENGTH, CELL_SIDE_LENGTH, 1000, 1000); // Arbitrary main grid dimensions
            mainGrid.Initialize(new TemperatureCell
            {
                temperature = 20, // Set initial temperature of all cells to 20 (TEMP, MOVE TO CONFIG)
                insulation = 0,
                heat = 0
            });

            Container.Bind<TemperatureService>()
                .FromNew()
                .AsSingle()
                .WithArguments(mainGrid, new BasicDenseGrid<TemperatureCell>(SIM_WINDOW_WIDTH, SIM_WINDOW_HEIGHT), _heatDiffusionShader);
        }
    }
}

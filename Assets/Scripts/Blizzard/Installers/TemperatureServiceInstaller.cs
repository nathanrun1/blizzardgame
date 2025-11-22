using Blizzard.Constants;
using UnityEngine;
using Zenject;
using Blizzard.Grid;
using Blizzard.Temperature;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class TemperatureServiceInstaller : MonoInstaller
    {
        private const int SIM_WINDOW_WIDTH = 32;
        private const int SIM_WINDOW_HEIGHT = 32;
        private const float CELL_SIDE_LENGTH = 0.5f;

        [SerializeField] private ComputeShader _heatDiffusionShader;
        [Header("Config")] [SerializeField] private bool _simulationIsActive = true;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            var mainGrid = new FullHashWorldGrid<TemperatureCell>(GameConstants.CellSideLength,GameConstants.CellSideLength)
                {
                    DefaultCell = new TemperatureCell
                    {
                        temperature = 5,
                        heat = 0,
                        insulation = 0,
                        ambient = 1
                    }
                };

            Container.BindInterfacesAndSelfTo<TemperatureService>()
                .FromNew()
                .AsSingle()
                .WithArguments(mainGrid, new BasicDenseGrid<TemperatureCell>(SIM_WINDOW_WIDTH, SIM_WINDOW_HEIGHT),
                    _heatDiffusionShader, _simulationIsActive);

            BLog.Log("Installed Temperature Service");
        }
    }
}
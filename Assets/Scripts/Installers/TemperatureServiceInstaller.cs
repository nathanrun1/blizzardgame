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
        [Header("Config")]
        [SerializeField] bool _simulationIsActive = true;

        public override void InstallBindings()
        {
            var mainGrid = new FullHashWorldGrid<TemperatureCell>(CELL_SIDE_LENGTH, CELL_SIDE_LENGTH);
            mainGrid.DefaultCell = new TemperatureCell
            {
                temperature = 5,
                heat = 0,
                insulation = 0,
                ambient = 1
            };

            Container.BindInterfacesAndSelfTo<TemperatureService>()
                .FromNew()
                .AsSingle()
                .WithArguments(mainGrid, new BasicDenseGrid<TemperatureCell>(SIM_WINDOW_WIDTH, SIM_WINDOW_HEIGHT), _heatDiffusionShader, _simulationIsActive);

            Debug.Log("Installed Temperature Service");
        }
    }
}

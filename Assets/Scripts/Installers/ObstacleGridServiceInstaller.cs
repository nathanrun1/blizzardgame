using UnityEngine;
using Zenject;
using Blizzard.Grid;

namespace Blizzard.Obstacles
{
    public class ObstacleGridServiceInstaller : MonoInstaller
    {
        const float CELL_SIDE_LENGTH = 0.5f;

        public override void InstallBindings()
        {
            Container.Bind<ObstacleGridService>()
                .FromNew()
                .AsSingle()
                .WithArguments(new HashWorldGrid<Obstacle>(CELL_SIDE_LENGTH, CELL_SIDE_LENGTH));
        }
    }
}

using UnityEngine;
using Zenject;
using Blizzard.Pathfinding;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class PathfindingServiceInstaller : MonoInstaller
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EnemyPathfindingService>()
                .FromNew()
                .AsSingle();

            BLog.Log("Installed Pathfinding Service");
        }
    }
}
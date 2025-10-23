using UnityEngine;
using Zenject;
using Blizzard.Pathfinding;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class PathfindingServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PathfindingService>()
                .FromNew()
                .AsSingle();

            BLog.Log("Installed Pathfinding Service");
        }
    }
}
using UnityEngine;
using Zenject;
using Blizzard.Pathfinding;

namespace Blizzard.Installers
{
    public class PathfindingServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PathfindingService>()
                .FromNew()
                .AsSingle();

            Debug.Log("Installed Pathfinding Service");
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Blizzard.Pathfinding
{
    public class InteractionServiceInstaller : MonoInstaller
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

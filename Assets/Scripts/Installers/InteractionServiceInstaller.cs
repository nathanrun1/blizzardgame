using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Blizzard.Grid;

namespace Blizzard.Obstacles
{
    public class InteractionServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<InteractionService>()
                .FromNew()
                .AsSingle();

            Debug.Log("Installed Interaction Service");
        }
    }
}

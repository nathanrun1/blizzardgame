using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Blizzard.Obstacles
{
    public class InteractionServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<InteractionService>()
                .AsSingle();

            Debug.Log("Installed Interaction Service");
        }
    }
}

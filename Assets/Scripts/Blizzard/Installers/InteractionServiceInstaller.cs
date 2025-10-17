using System;
using UnityEngine;
using Zenject;
using Blizzard.Obstacles;

namespace Blizzard.Installers
{
    public class InteractionServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InteractionService>()
                .AsSingle();

            Debug.Log("Installed Interaction Service");
        }
    }
}

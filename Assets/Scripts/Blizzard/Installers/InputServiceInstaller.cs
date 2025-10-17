using UnityEngine;
using Zenject;
using Blizzard.Grid;
using Blizzard.Input;

namespace Blizzard.Installers
{
    public class InputServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputService>()
                .AsSingle();

            Debug.Log("Installed Input Service");
        }
    }
}

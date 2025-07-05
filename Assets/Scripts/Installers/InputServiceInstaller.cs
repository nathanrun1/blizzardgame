using UnityEngine;
using Zenject;
using Blizzard.Grid;

namespace Blizzard
{
    public class InputServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<InputService>()
                .AsSingle();
        }
    }
}

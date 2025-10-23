using Zenject;
using Blizzard.Input;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class InputServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputService>()
                .AsSingle();

            BLog.Log("Installed Input Service");
        }
    }
}
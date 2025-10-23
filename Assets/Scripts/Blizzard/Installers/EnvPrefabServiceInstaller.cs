using Blizzard.Utilities;
using UnityEngine;
using Zenject;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class EnvPrefabServiceInstaller : MonoInstaller
    {
        [SerializeField] private EnvironmentDatabase _environmentDatabase;
        [SerializeField] private Transform _environmentParent;

        public override void InstallBindings()
        {
            Container.Bind<EnvPrefabService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_environmentDatabase, _environmentParent);

            BLog.Log("Installed EnvPrefab Service");
        }
    }
}
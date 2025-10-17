using Blizzard.Utilities;
using UnityEngine;
using Zenject;

namespace Blizzard.Installers
{
    public class EnvPrefabServiceInstaller : MonoInstaller
    {
        [SerializeField] EnvironmentDatabase _environmentDatabase;
        [SerializeField] Transform _environmentParent;

        public override void InstallBindings()
        {
            Container.Bind<EnvPrefabService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_environmentDatabase, _environmentParent);

            Debug.Log("Installed EnvPrefab Service");
        }
    }
}

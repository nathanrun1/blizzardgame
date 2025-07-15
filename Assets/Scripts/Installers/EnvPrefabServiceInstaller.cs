using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Blizzard.Utilities
{
    public class EnvPrefabServiceInstaller : MonoInstaller
    {
        [SerializeField] EnvironmentDatabase _environmentDatabase;
        [SerializeField] Transform _environmentParent;

        public override void InstallBindings()
        {
            Debug.Log("Installing environment prefab service...");
            Container.Bind<EnvPrefabService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_environmentDatabase, _environmentParent);
        }
    }
}

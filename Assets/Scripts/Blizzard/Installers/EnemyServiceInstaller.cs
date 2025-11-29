using Blizzard.NPCs.Core;
using Blizzard.NPCs.Spawning;
using UnityEngine;
using Zenject;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class EnemyServiceInstaller : MonoInstaller
    {
        [SerializeField] private Transform _enemiesParent;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NPCService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_enemiesParent);

            Container.BindInterfacesAndSelfTo<EnemyWaveService>()
                .FromNew()
                .AsSingle();

            BLog.Log("Installed Enemy Service and Enemy Wave Service");
        }
    }
}
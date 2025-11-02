using UnityEngine;
using Zenject;
using Blizzard.Enemies.Core;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class EnemyServiceInstaller : MonoInstaller
    {
        [SerializeField] private Transform _enemiesParent;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EnemyService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_enemiesParent);

            BLog.Log("Installed Enemy Service");
        }
    }
}
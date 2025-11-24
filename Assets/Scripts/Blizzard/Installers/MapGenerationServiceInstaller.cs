using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Blizzard.Obstacles;
using Blizzard.Resources;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class MapGenerationServiceInstaller : MonoInstaller
    {
        [SerializeField] private List<ResourceGenInfo> _resourcesToGenerate;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MapGenerationService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_resourcesToGenerate);

            BLog.Log("Installed Map Generation Service");
        }
    }
}
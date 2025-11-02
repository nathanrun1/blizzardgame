using UnityEngine;
using Zenject;
using Blizzard.Grid;
using Blizzard.UI.Core;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class UIServiceInstaller : MonoInstaller
    {
        [SerializeField] private RectTransform _uiParent;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            Container.Bind<UIService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_uiParent);

            BLog.Log("Installed UI Service");
        }
    }
}
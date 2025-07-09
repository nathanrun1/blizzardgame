using UnityEngine;
using Zenject;
using Blizzard.Grid;
using Blizzard.UI;

namespace Blizzard.UI
{
    public class UIServiceInstaller : MonoInstaller
    {
        [SerializeField] private UIDatabase _uiDatabase;
        [SerializeField] private RectTransform _uiParent;

        public override void InstallBindings()
        {
            Container.Bind<UIService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_uiDatabase, _uiParent);
        }
    }
}

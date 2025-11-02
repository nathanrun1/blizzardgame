using UnityEngine;
using Zenject;
using Blizzard.Inventory;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class InventoryServiceInstaller : MonoInstaller
    {
        [SerializeField] private int slotAmount = 36; // TEMP, MOVE TO CONFIG!

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            Container.Bind<InventoryService>()
                .FromNew()
                .AsSingle()
                .WithArguments(slotAmount);

            BLog.Log("Installed Inventory Service");
        }
    }
}
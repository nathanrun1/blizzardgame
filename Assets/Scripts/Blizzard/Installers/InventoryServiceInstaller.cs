using UnityEngine;
using Zenject;
using Blizzard.Inventory;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class InventoryServiceInstaller : MonoInstaller
    {
        [SerializeField] private int slotAmount = 36; // TEMP, MOVE TO CONFIG!

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
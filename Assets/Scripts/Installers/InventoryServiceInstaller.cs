using UnityEngine;
using Zenject;

namespace Blizzard.Inventory
{
    public class InventoryServiceInstaller : MonoInstaller
    {
        [SerializeField] int slotAmount = 36; // TEMP, MOVE TO CONFIG!

        public override void InstallBindings()
        {
            Container.Bind<InventoryService>()
                .FromNew()
                .AsSingle()
                .WithArguments(slotAmount);

            Debug.Log("Installed Inventory Service");
        }
    }
}

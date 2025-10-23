using Blizzard.Inventory.Crafting;
using UnityEngine;
using Zenject;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    /// <summary>
    /// Responsible for installing global config scriptable objects
    /// </summary>
    public class ConfigInstaller : MonoInstaller
    {
        [Header("Config Objects")] [SerializeField]
        private CraftingDatabase _craftingDatabase;

        [SerializeField] private SmeltingDatabase _smeltingDatabase;

        public override void InstallBindings()
        {
            // Crafting database
            Container.BindInstance(_craftingDatabase).AsSingle();

            // Smelting database
            Container.BindInstance(_smeltingDatabase).AsSingle();

            BLog.Log("Installed Configs");
        }
    }
}
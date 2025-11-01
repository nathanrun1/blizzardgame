using Blizzard.Inventory.Crafting;
using Blizzard.UI.Core;
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
        [Header("Config Objects")]
        [SerializeField] private CraftingDatabase _craftingDatabase;
        [SerializeField] private SmeltingDatabase _smeltingDatabase;
        [SerializeField] private UIDatabase _uiDatabase;

        public override void InstallBindings()
        {
            // Crafting database
            Container.BindInstance(_craftingDatabase).AsSingle();

            // Smelting database
            Container.BindInstance(_smeltingDatabase).AsSingle();
            
            // UI database
            Container.BindInstance(_uiDatabase).AsSingle();

            BLog.Log("Installed Configs");
        }
    }
}
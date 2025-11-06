using Blizzard.Config;
using Blizzard.Enemies.Core;
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
        [SerializeField] private EnemyDatabase _enemyDatabase;
        [SerializeField] private PlayerTemperatureConfig _playerTemperatureConfig;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            // Crafting database
            Container.BindInstance(_craftingDatabase).AsSingle();

            // Smelting database
            Container.BindInstance(_smeltingDatabase).AsSingle();
            
            // UI database
            Container.BindInstance(_uiDatabase).AsSingle();
            
            // Enemy database
            Container.BindInstance(_enemyDatabase).AsSingle();
            
            // Player temperature config
            Container.BindInstance(_playerTemperatureConfig).AsSingle();

            BLog.Log("Installed Configs");
        }
    }
}
using Blizzard.Config;
using Blizzard.Inventory.Crafting;
using Blizzard.NPCs.Core;
using Blizzard.UI.Core;
using UnityEngine;
using Zenject;
using Blizzard.Utilities.Logging;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("_enemyDatabase")] [SerializeField] private NPCDatabase npcDatabase;
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
            Container.BindInstance(npcDatabase).AsSingle();
            
            // Player temperature config
            Container.BindInstance(_playerTemperatureConfig).AsSingle();

            BLog.Log("Installed Configs");
        }
    }
}
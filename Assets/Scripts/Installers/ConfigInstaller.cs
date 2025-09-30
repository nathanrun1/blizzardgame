using Blizzard.Inventory.Crafting;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Blizzard.Config
{
    /// <summary>
    /// Responsible for installing global config scriptable objects
    /// </summary>
    public class ConfigInstaller : MonoInstaller
    {
        [Header("Config Objects")]
        [SerializeField] CraftingDatabase _craftingDatabase;
        [SerializeField] SmeltingDatabase _smeltingDatabase;

        public override void InstallBindings()
        {
            // Crafting database
            Container.BindInstance(_craftingDatabase).AsSingle();

            // Smelting database
            Container.BindInstance(_smeltingDatabase).AsSingle();

            Debug.Log("Installed Configs");
        }
    }
}

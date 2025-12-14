using Blizzard.Config;
using Blizzard.Input;
using Blizzard.Inventory;
using Blizzard.ItemTypes;
using Blizzard.Utilities;
using Blizzard.Utilities.Assistants;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Zenject;

namespace Blizzard.Player
{
    /// <summary>
    /// Manages player's worn clothing
    /// </summary>
    public class ClothingService : IInitializable
    {
        [Inject] private InventoryService _inventoryService;
        [Inject] private InputService _inputService;
        [Inject] private EnvPrefabService _envPrefabService;
        [Inject] private PlayerService _playerService;
        [Inject] private PlayerTemperatureConfig _config;

        public ClothingItemData CurrentClothing { get; private set; }

        private ClothingItemData _curEquipped;
        private int _curEquippedSlotIndex;

        public void Initialize()
        {
            WearClothing(_config.defaultClothing);
            _inputService.inputActions.Player.Fire.performed += OnInputFire;
        }

        /// <summary>
        /// Invoked when player uses 'fire' input. If clothing item is equipped (held), swaps current worn clothing with
        /// equipped clothing.
        /// </summary>
        private void OnInputFire(InputAction.CallbackContext ctx)
        {
            if (!_curEquipped) return;
            if (InputAssistant.IsPointerOverUIElement()) return;
            Assert.IsTrue(_inventoryService.CountOfItem(_curEquipped) >= 1, "Attempted to wear clothing that isn't present in inventory!");

            ClothingItemData removedClothing = CurrentClothing;
            WearClothing(_curEquipped);
            if (_inventoryService.TryRemoveItemAt(_curEquippedSlotIndex, 1) < 1) return;

            if (_inventoryService.TryAddItem(removedClothing) < 1)
                InventoryServiceExtensions.DropItem(_envPrefabService, _playerService, removedClothing, 1);
        }
        
        public void OnClothingItemEquipped(ClothingItemData itemData, EquipData equipData)
        {
            _curEquipped = itemData;
            _curEquippedSlotIndex = equipData.slotIndex;
        }

        public void OnClothingItemUnequipped(ClothingItemData itemData)
        {
            _curEquipped = null;
            _curEquippedSlotIndex = -1;
        }

        private void WearClothing(ClothingItemData itemData)
        {
            CurrentClothing = itemData;
            _playerService.SetPlayerSprite(itemData.playerSprite);
        }
    }
}
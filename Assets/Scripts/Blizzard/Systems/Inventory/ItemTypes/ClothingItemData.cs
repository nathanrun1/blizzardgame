using Blizzard.Input;
using Blizzard.Inventory;
using Blizzard.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Blizzard.ItemTypes
{
    [CreateAssetMenu(fileName = "ClothingItemData", menuName = "ScriptableObjects/Inventory/ClothingItemData")]
    public class ClothingItemData : ItemData
    {
        [Inject] private ClothingService _clothingService;
        
        /// <summary>
        /// Category of this item, determines how its used and its attributes
        /// </summary>
        public override ItemCategory category { get; set; } = ItemCategory.Clothing;
        
        [Header("Clothing Config")]
        /// <summary>
        /// Insulation provided to the player when worn
        /// </summary>
        public float insulation;
        
        /// <summary>
        /// Player sprite to be used when worn
        /// </summary>
        public Sprite playerSprite;
        

        public override void Equip(EquipData equipData)
        {
            _clothingService.OnClothingItemEquipped(this, equipData);
            base.Equip(equipData);
        }

        public override void Unequip()
        {
            _clothingService.OnClothingItemUnequipped(this);
            base.Unequip();
        }
    }
}
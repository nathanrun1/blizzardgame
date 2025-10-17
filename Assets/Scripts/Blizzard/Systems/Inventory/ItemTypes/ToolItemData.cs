using UnityEngine;
using Blizzard.Player;
using Blizzard.Tools;
using Zenject;

namespace Blizzard.Inventory.Itemtypes
{
    [CreateAssetMenu(fileName = "ToolItemData", menuName = "ScriptableObjects/Inventory/ToolItemData")]
    public class ToolItemData : ItemData
    {
        public ToolBehaviour toolPrefab;

        /// <summary>
        /// Category of this item, determines how its used and its attributes
        /// </summary>
        [HideInInspector] public override ItemCategory category { get; set; } = ItemCategory.Tool;

        [Inject] PlayerService _playerService;

        public override void Equip(EquipData _)
        {
            _playerService.EquipTool(this);
        }

        public override void Unequip()
        {
            _playerService.UnequipTool();
        }
    }
}
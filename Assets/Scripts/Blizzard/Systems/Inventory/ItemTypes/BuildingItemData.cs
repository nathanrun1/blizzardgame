using UnityEngine;
using Zenject;
using Blizzard.Building;
using Blizzard.UI;
using Blizzard.UI.Core;
using Blizzard.Utilities.Logging;

namespace Blizzard.Inventory.ItemTypes
{
    /// <summary>
    /// A building in the form of an item
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingItemData", menuName = "ScriptableObjects/Inventory/BuildingItemData")]
    public class BuildingItemData : ItemData
    {
        public BuildingData buildingData;

        /// <summary>
        /// Category of this item, determines how its used and its attributes
        /// </summary>
        [HideInInspector]
        public override ItemCategory category { get; set; } = ItemCategory.Tool;

        [Inject] private UIService _uiService;

        public override void Equip(EquipData equipData)
        {
            BLog.Log($"Equipping {this}");
            _uiService.InitUI(UIID.Build, new BuildUI.Args
            {
                buildingData = buildingData,
                itemSlot = equipData.slotIndex
            });
        }

        public override void Unequip()
        {
            BLog.Log($"Unequipping {this}");
            _uiService.CloseUI(UIID.Build);
        }
    }
}
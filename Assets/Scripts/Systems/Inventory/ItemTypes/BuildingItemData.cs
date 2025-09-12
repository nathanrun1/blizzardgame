using UnityEngine;
using Zenject;
using Blizzard.Building;
using Blizzard.UI;

namespace Blizzard.Inventory
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
        [HideInInspector] public override ItemCategory category { get; set; } = ItemCategory.Tool;

        [Inject] UIService _uiService;

        public override void Equip(EquipData equipData)
        {
            _uiService.InitUI("build", new BuildUI.Args
            {
                buildingData = buildingData,
                itemSlot = equipData.slotIndex
            });
        }

        public override void Unequip()
        {
            _uiService.CloseUI("build");
        }
    }
}
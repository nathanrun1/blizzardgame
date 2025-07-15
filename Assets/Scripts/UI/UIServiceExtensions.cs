using Blizzard.Inventory;
using Blizzard.UI;
using Unity.Burst;
using UnityEngine;

namespace Blizzard.Inventory
{
    [BurstCompile]
    public static class UIServiceExtensions
    {
        /// <summary>
        /// Initialize item gain animation UI with given item, amount and collection position
        /// </summary>
        public static void ItemGain(this UIService uiService, ItemData item, int amount, Vector3 collectPosition)
        {
            uiService.InitUI("item_gain", new ItemGainUI.Args
            {
                item = item,
                amount = amount,
                worldPosition = collectPosition
            });
        }
    }
}

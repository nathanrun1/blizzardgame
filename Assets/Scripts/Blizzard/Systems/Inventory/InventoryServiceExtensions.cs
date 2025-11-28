using System.Collections.Generic;
using Blizzard.Constants;
using UnityEngine;
using Blizzard.UI.Core;
using Blizzard.Environment;
using Blizzard.Player;
using Blizzard.UI;
using Blizzard.Utilities;

namespace Blizzard.Inventory
{
    public static class InventoryServiceExtensions
    {
        /// <summary>
        /// Initializes and adds as many items from the given list of ItemAmountPairs as possible.
        /// Plays UI animation for each item successfully added on given UI service
        /// Post: Given list now holds remaining items that couldn't be added due to player inventory being full.
        /// </summary>
        /// <param name="inventoryService">Inventory Service</param>
        /// <param name="uiService">UI Service</param>
        /// <param name="collectPosition">Position that the item was collected at</param>
        /// <param name="items">Items to add, modified during invocation to contain items that couldn't be added</param>
        public static void TryAddItemsWithAnim(this InventoryService inventoryService, UIService uiService,
            Vector3 collectPosition, List<ItemAmountPair> items)
        {
            List<ItemAmountPair> addedItems = new();
            for (var i = 0; i < items.Count; ++i)
            {
                var pair = items[i];
                var added = inventoryService.TryAddItem(pair.item, pair.amount, true);

                // Update pair
                items[i] = new ItemAmountPair { item = pair.item, amount = pair.amount - added };

                // Update added
                addedItems.Add(new ItemAmountPair { item = pair.item, amount = added });
            }

            // Remove all pairs with amount now 0 (all items from that pair were added)
            items.RemoveAll(pair => pair.amount == 0);

            uiService.ItemGain(addedItems, collectPosition);
        }

        /// <summary>
        /// Attempts to add given amount of given item to first available inventory slots.
        /// Plays the item gain UI animation for items successfully added.
        /// </summary>
        /// <returns>Amount of given item successfully added</returns>
        public static int TryAddItemWithAnim(this InventoryService inventoryService, UIService uiService,
            Vector3 collectPosition, ItemData item, int amount, bool fill = true)
        {
            var added = inventoryService.TryAddItem(item, amount, fill);
            uiService.ItemGain(item, added, collectPosition);
            return added;
        }

        /// <summary>
        /// Drops given amount of given item in front of the player. Does not affect inventory contents.
        /// </summary>
        public static void DropItem(EnvPrefabService envPrefabService, PlayerService playerService, ItemData item,
            int amount)
        {
            var dropObj = envPrefabService.InstantiatePrefab("item_drop").GetComponent<ItemDrop>();

            var plrFacingDirection = playerService.GetFacingDirection();
            dropObj.transform.position = playerService.PlayerPosition +
                                         new Vector2(plrFacingDirection.x, plrFacingDirection.y) *
                                         InventoryConstants.ItemDropDistance;

            dropObj.Setup(new ItemAmountPair
                { item = item, amount = amount }); // (amountRemoved should be one, failsafe regardless)
        }
    }
}
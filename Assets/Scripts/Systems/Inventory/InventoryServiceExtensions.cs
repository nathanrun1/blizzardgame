using System.Collections.Generic;
using UnityEngine;
using Blizzard.UI;

namespace Blizzard.Inventory
{
    /// <summary>
    /// Initializes and adds as many items from the given list of ItemAmountPairs as possible.
    /// Plays UI animation for each item successfully added on given UI service
    /// Post: Given list now holds remaining items that couldn't be added due to player inventory being full.
    /// </summary>
    /// <param name="items">Items to add, modified during invocation to contain items that couldn't be added</param>
    public static class InventoryServiceExtensions
    {
        public static void TryAddItemsWithAnim(this InventoryService inventoryService, UIService uiService, Vector3 collectPosition, List<ItemAmountPair> items)
        {
            List<ItemAmountPair> addedItems = new();
            for (int i = 0; i < items.Count; ++i)
            {
                ItemAmountPair pair = items[i];
                int added = inventoryService.TryAddItem(pair.item, pair.amount, fill: true);

                // Update pair
                items[i] = new ItemAmountPair { item = pair.item, amount = pair.amount - added };

                // Update added
                addedItems.Add(new ItemAmountPair { item = pair.item, amount = added });
            }

            // Remove all pairs with amount now 0 (all items from that pair were added)
            items.RemoveAll(pair => pair.amount == 0);

            uiService.ItemGain(addedItems, collectPosition);
        }
    }
}
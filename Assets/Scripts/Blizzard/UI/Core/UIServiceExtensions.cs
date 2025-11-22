using System;
using System.Collections.Generic;
using Blizzard.Inventory;
using Unity.Burst;
using UnityEngine;
using Blizzard.UI.Core;

namespace Blizzard.UI
{
    [BurstCompile]
    public static class UIServiceExtensions
    {
        /// <summary>
        /// Initialize item gain animation UI with given item, amount and collection position
        /// </summary>
        public static void ItemGain(this UIService uiService, ItemData item, int amount, Vector3 collectPosition)
        {
            // TODO: use a queue for overlapping item gains?
            uiService.InitUI(UIID.ItemGain, new ItemGainUI.Args
            {
                item = item,
                amount = amount,
                worldPosition = collectPosition
            });
        }

        /// <summary>
        /// Initialize item gain animation UI with given list of items, amount and collection position
        /// </summary>
        /// <param name="uniqueAnimDelay">Delay between animation for each unique item type in given list</param>
        public static void ItemGain(this UIService uiService, List<ItemAmountPair> items, Vector3 collectPosition,
            float uniqueAnimDelay = 0.25f)
        {
            if (items.Count == 1)
            {
                ItemGain(uiService, items[0].item, items[0].amount, collectPosition);
                return;
            }

            Action next = null;
            items.Reverse();
            foreach (var itemAmountPair in items)
            {
                var capturedNext = next;
                Action temp = () =>
                {
                    uiService.InitUI(UIID.ItemGain, new ItemGainUI.Args
                    {
                        item = itemAmountPair.item,
                        amount = itemAmountPair.amount,
                        worldPosition = collectPosition,
                        OnAnimComplete = capturedNext
                    });
                };
                next = temp;
            }

            next?.Invoke();
        }
    }
}
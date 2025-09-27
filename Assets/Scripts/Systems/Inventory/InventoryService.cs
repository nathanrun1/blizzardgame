using System;
using System.Collections.Generic;
using UnityEngine;
using ModestTree;
using Blizzard.UI;
using Zenject;

namespace Blizzard.Inventory
{
    /// <summary>
    /// A slot in the player's inventory.
    /// </summary>
    public class InventorySlot
    {
        /// <summary>
        /// Invoked when slot's contents are updated
        /// </summary>
        public event Action OnUpdate;

        /// <summary>
        /// Item contained within the slot, null if slot is empty
        /// </summary>
        public ItemData Item
        {
            get => _item;
            set
            {
                _item = value;
                Debug.Log($"Updated item in slot: {_item}");
                Debug.Log(OnUpdate != null ? OnUpdate.GetInvocationList().Length : "0");
                OnUpdate?.Invoke();
            }
        }
        private ItemData _item = null;
        /// <summary>
        /// Amount of specified item contained within the slot
        /// </summary>
        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                Debug.Log($"Updated amount in slot. {_amount}");
                Debug.Log(OnUpdate != null ? OnUpdate.GetInvocationList().Length : "0");
                OnUpdate?.Invoke();
            }
        }
        private int _amount = 0;
    }

    /// <summary>
    /// An item and an associated amount
    /// </summary>
    [Serializable]
    public struct ItemAmountPair
    {
        public ItemData item;
        public int amount;
    }

    public static class InventorySlotExtensions 
    { 
        public static bool Empty(this InventorySlot slot)
        {
            return slot.Item == null || slot.Amount == 0;
        }
    }

    public class InventoryService
    {
        public List<InventorySlot> inventorySlots { get; private set; }

        public ItemData equippedItem = null;

        /// <summary>
        /// Event that's invoked when the inventory is modified.
        /// Invoked once per slot modified.
        /// Args: (index of modified slot in 'inventorySlots': int)
        /// </summary>
        public event Action<int> OnInventoryModified;

        public InventoryService(int slotAmount)
        {
            inventorySlots = new List<InventorySlot>();
            for (int i = 0; i < slotAmount; ++i)
            {
                var slot = new InventorySlot();
                int j = i; // Make copy of index for the lambda
                slot.OnUpdate += () =>
                {
                    OnInventoryModified?.Invoke(j);
                };
                inventorySlots.Add(slot);
            }
        }

        /// <summary>
        /// Attempts to add given amount of given item to first available slots.
        /// </summary>
        /// <param name="fill">If and only if set to true, will only add as many as can fit in the inventory, even if not enough space for given amount.</param>
        /// <returns>Amount of item successfully added</returns>
        public int TryAddItem(ItemData item, int amount = 1, bool fill = true)
        {
            if (!fill && !CanFitItem(item, amount)) return 0; // If not set to fill and can't fit all items, don't add any

            int leftToAdd = amount;
            for (int i = 0; i < inventorySlots.Count; ++i)
            {
                InventorySlot slot = inventorySlots[i];

                if (slot.Empty() || item == slot.Item)
                {
                    // Add max(stacksize - amount in slot, amount left to add)
                    int amountToAdd = Math.Min(item.stackSize - slot.Amount, Math.Max(leftToAdd, 0));
                    slot.Amount += amountToAdd;
                    leftToAdd -= amountToAdd;

                    if (slot.Empty()) slot.Item = item;

                    Debug.Log($"Added {amountToAdd}x {item.displayName}!");

                    if (leftToAdd <= 0) break;
                }
            }
            if (fill) Assert.That(leftToAdd >= 0, "More than given amount was added! Likely an implementation error in InventoryService.");
            else Assert.That(leftToAdd == 0, "Not exactly the given amount added, yet not set to fill! Likely an implementation error in InventoryService.");
            return amount - leftToAdd;
        }

        /// <summary>
        /// Initializes and adds as many items from the given list of ItemAmountPairs as possible.
        /// Post: Given list now holds remaining items that couldn't be added due to player inventory being full.
        /// </summary>
        /// <param name="items">Items to add, modified during invocation to contain items that couldn't be added</param>
        public void TryAddItems(List<ItemAmountPair> items)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                ItemAmountPair pair = items[i];
                int added = TryAddItem(pair.item, pair.amount, fill: true);

                // Update pair
                items[i] = new ItemAmountPair { item = pair.item, amount = pair.amount - added };
            }

            // Remove all pairs with amount now 0 (all items from that pair were added)
            items.RemoveAll(pair => pair.amount == 0);
        }

        /// <summary>
        /// Checks if inventory can fit given amount of given item
        /// </summary>
        public bool CanFitItem(ItemData item, int amount = 1)
        {
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.Empty() || slot.Item.Equals(item))
                {
                    int amountCanAdd = Math.Max(item.stackSize - slot.Amount, 0);
                    amount -= amountCanAdd;
                    if (amount <= 0) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Counts how many of a given item is in the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int CountOfItem(ItemData item)
        {
            int count = 0;
            foreach (InventorySlot slot in inventorySlots)
            {
                if (item == slot.Item) count += slot.Amount;
            }
            return count;
        }

        /// <summary>
        /// Checks if inventory contains at least all of these items
        /// </summary>
        public bool HasItems(List<ItemAmountPair> items)
        {
            // Convert to dictionary for efficient checks
            Dictionary<int, int> itemAmounts = new Dictionary<int, int>();
            foreach (ItemAmountPair pair in items)
            {
                if (!itemAmounts.ContainsKey(pair.item.id)) itemAmounts.Add(pair.item.id, pair.amount);
                else itemAmounts[pair.item.id] += pair.amount;
            }

            foreach (InventorySlot slot in inventorySlots)
            {
                if (!slot.Empty() && itemAmounts.ContainsKey(slot.Item.id))
                {
                    itemAmounts[slot.Item.id] -= slot.Amount;
                }
            }


            // Inventory has all items if all dictionary entries have non-positive value
            foreach (KeyValuePair<int, int> pair in itemAmounts)
            {
                if (pair.Value > 0) return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to remove given amount of given item.
        /// </summary>
        /// <param name="drain">If and only if set to true, will only remove as many as possible, even if not enough to satisfy given amount.</param>
        /// <returns>Amount of item successfully removed.</returns>
        public int TryRemoveItem(ItemData item, int amount = 1, bool drain = false)
        {
            if (!drain && CountOfItem(item) < amount)
            {
                Debug.Log("Drain set to false and not enough of item to remove, 0 removed!");
                return 0; // 'drain' set to false and not enough available to be removed.
            }

            int leftToRemove = amount;
            for (int i = 0; i < inventorySlots.Count; ++i)
            {
                InventorySlot slot = inventorySlots[i];

                if (item == slot.Item)
                {
                    // Remove min(amount in slot, amount left to remove)
                    int amountToRemove = Math.Min(slot.Amount, leftToRemove);
                    Debug.Log($"Removing {amountToRemove}x {item.displayName}");

                    slot.Amount -= amountToRemove;
                    if (slot.Amount == 0) slot.Item = null;

                    leftToRemove -= amountToRemove;
                    if (leftToRemove <= 0) break;
                }
            }

            if (drain) Assert.That(leftToRemove >= 0, "More than given amount was removed! Likely implementation error in InventoryService.");
            else Assert.That(leftToRemove == 0, "Not exactly the given amount removed, yet not set to drain! Likely implementation error in InventoryService.");
            return amount - leftToRemove;
        }

        /// <summary>
        /// Attempts to remove given amount of item from a single slot
        /// </summary>
        /// <param name="slotIndex">Index of slot to remove from</param>
        /// <param name="amount">Amount to remove</param>
        /// <param name="drain">Iff set to true and can't remove entire amount, will remove as much as possible instead of none at all.</param>
        /// <returns>Amount successfully removed</returns>
        public int TryRemoveItemAt(int slotIndex, int amount, bool drain = false)
        {
            Assert.That(0 <= slotIndex && slotIndex <= inventorySlots.Count);

            InventorySlot slot = inventorySlots[slotIndex];

            if (slot.Item == null) return 0; // No items to remove
            if (!drain && slot.Amount < amount) return 0; // Not enough to remove

            int amountToRemove = Math.Min(slot.Amount, amount);
            slot.Amount -= amountToRemove;

            if (slot.Amount == 0) slot.Item = null;

            return amountToRemove;
        }

        /// <summary>
        /// Attempts to remove given list of item/amount pairs. If not enough in inventory to remove, has no effect.
        /// </summary>
        /// <returns>True if succesfully removed items, false otherwise</returns>
        public bool TryRemoveItems(List<ItemAmountPair> items)
        {
            // Convert to dictionary for efficient checks
            Dictionary<int, int> itemAmounts = new Dictionary<int, int>();
            foreach (ItemAmountPair pair in items)
            {
                if (!itemAmounts.ContainsKey(pair.item.id)) itemAmounts.Add(pair.item.id, pair.amount);
                else itemAmounts[pair.item.id] += pair.amount;
            }

            int[] toRemove = new int[inventorySlots.Count]; // Amount to remove from each slot

            // Check that there is enough in inventory to remove (effectively HasItems(items))
            for (int i = 0; i < inventorySlots.Count; ++i)
            {
                InventorySlot slot = inventorySlots[i];
                if (slot.Empty() || !itemAmounts.ContainsKey(slot.Item.id)) continue;

                int amountToRemove = Math.Min(slot.Amount, itemAmounts[slot.Item.id]);
                itemAmounts[slot.Item.id] -= amountToRemove;

                // Remember to remove this much from this slot
                toRemove[i] += amountToRemove; 
            }

            // Inventory has all items if all dictionary entries are now zero
            foreach (KeyValuePair<int, int> pair in itemAmounts)
            {
                if (pair.Value != 0) return false;
            }

            // Remove amounts from slots
            for (int i = 0; i < inventorySlots.Count; ++i)
            {
                InventorySlot slot = inventorySlots[i];
                slot.Amount -= toRemove[i];
                if (slot.Amount == 0) slot.Item = null;

                Assert.That(slot.Amount >= 0, "Removed more of an item than there was in inventory! Likely an implementation error.");
            }

            return true;
        }

        /// <summary>
        /// "Equips" the item at the given slot index, unequips any currently equipped item
        /// </summary>
        public void EquipItem(int slotIndex)
        {
            Assert.That(0 <= slotIndex && slotIndex <= inventorySlots.Count, $"Invalid slot index given ({slotIndex}) when attempting to equip item! # Slots: " + inventorySlots.Count);

            UnequipItem();

            equippedItem = inventorySlots[slotIndex].Item;
            if (equippedItem != null) equippedItem.Equip(new EquipData { slotIndex = slotIndex});
        }


        /// <summary>
        /// Unequips the currently equipped item, if any
        /// </summary>
        public void UnequipItem()
        {
            if (equippedItem == null) return;

            equippedItem.Unequip();
            equippedItem = null;
        }
    }
}

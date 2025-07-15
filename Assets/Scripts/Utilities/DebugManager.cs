using UnityEngine;
using Zenject;
using Blizzard.Inventory;
using Sirenix.OdinInspector;
using Blizzard.UI;

public class DebugManager : MonoBehaviour
{
    [Inject] InventoryService _inventoryService;
    [Inject] UIService _uiService;

    [FoldoutGroup("UI")]
    [Button]
    private void InitUI(int id)
    {
        _uiService.InitUI(id);
    }
    [FoldoutGroup("UI")]
    [Button]
    private void CloseUI(int id)
    {
        _uiService.CloseUI(id);
    }

    [FoldoutGroup("Inventory")]
    [Button]
    private void InventoryAddItem(ItemData item, int amount, bool fill = true)
    {
        int added = _inventoryService.TryAddItem(item, amount, fill);
        Debug.Log($"Successfully added {added}x {item.displayName}");
    }
    [FoldoutGroup("Inventory")]
    [Button]
    private void InventoryRemoveItem(ItemData item, int amount, bool drain = false)
    {
        Debug.Log(item == null);
        int removed = _inventoryService.TryRemoveItem(item, amount, drain);
        Debug.Log($"Successfully removed {removed}x {item.displayName}");
    }
    [FoldoutGroup("Inventory")]
    [Button]
    private void PrintInventoryContents()
    {
        string str = "--INVENTORY--\n";
        foreach (InventorySlot slot in _inventoryService.inventorySlots)
        {
            if (slot.Empty()) str += "[EMPTY]\n";
            else str += $"[{slot.amount}x {slot.item.displayName}]\n";
        }
        Debug.Log(str);
    }
}

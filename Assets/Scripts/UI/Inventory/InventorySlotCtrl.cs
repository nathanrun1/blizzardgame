using Blizzard.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotCtrl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image _itemIcon;
    [SerializeField] TextMeshProUGUI _itemCount;
    [SerializeField] Image _bgSelected;
        
    /// <summary>
    /// Setup this slot with given item and amount
    /// </summary>
    public void Setup(ItemData item, int amount)
    {
        if (item != null && amount > 0)
        {
            // Slot not empty
            _itemIcon.enabled = true;
            _itemIcon.sprite = item.icon;
            _itemCount.text = amount != 1 ? amount.ToString() : ""; // Don't show amount if only 1
        }
        else
        {
            // Slot empty
            _itemIcon.enabled = false;
            _itemCount.text = "";
        }
    }

    /// <summary>
    /// Set whether this slot is currently selected
    /// </summary>
    public void SetSelected(bool selected)
    {
        _bgSelected.gameObject.SetActive(selected);
    }
}

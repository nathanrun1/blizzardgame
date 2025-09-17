using Blizzard.Inventory;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class InventorySlotCtrl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image _itemIcon;
    [SerializeField] TextMeshProUGUI _itemCount;
    [SerializeField] Image _bgSelected;
    [SerializeField] Image _itemPreviewIcon;

    public Button slotButton;

    private InventorySlot _linkedSlot;
        
    /// <summary>
    /// Setup this slot with given item and amount
    /// </summary>
    public void Setup(ItemData item, int amount, bool isPreview = false)
    {
        if (item != null && amount > 0)
        {
            // Slot not empty
            if (isPreview)
            {
                // Item preview 
                _itemIcon.enabled = false;
                _itemPreviewIcon.enabled = true;
                _itemPreviewIcon.sprite = item.icon;
            } 
            else
            {
                // Actual item
                _itemPreviewIcon.enabled = false;
                _itemIcon.enabled = true;
                _itemIcon.sprite = item.icon;
            }
            _itemCount.text = amount != 1 ? amount.ToString() : ""; // Don't show amount if only 1
        }
        else
        {
            // Slot empty
            _itemIcon.enabled = false;
            _itemPreviewIcon.enabled = false;
            _itemCount.text = "";
        }
    }

    /// <summary>
    /// Setup this slot by linking it to an InventorySlot instance.
    /// The inventory slot UI will automatically update when the linked slot is updated.
    /// Also allows the ability for moving items between slots through the UI.
    /// </summary>
    public void LinkedSetup(InventorySlot slot)
    {
        if (_linkedSlot != null)
        {
            Debug.LogWarning("[InventorySlotCtrl] This slot has already been linked to a different InventorySlot! Unlinking...");
            _linkedSlot.OnUpdate -= OnLinkedSlotUpdated;
        }
        Setup(slot.Item, slot.Amount, false);
        _linkedSlot = slot;
        _linkedSlot.OnUpdate += OnLinkedSlotUpdated;
    }

    /// <summary>
    /// Unlinks this slot from its linked InventorySlot instance, if it has been linked.
    /// </summary>
    /// <param name="setupEmpty">Whether to clear the UI for this slot (i.e. set it to appear empty)</param>
    public void Unlink(bool setupEmpty = true)
    {
        if (_linkedSlot == null)
        {
            Debug.LogWarning("[InventorySlotCtrl] Unlink() called, yet not linked!");
            return;
        }
        _linkedSlot.OnUpdate -= OnLinkedSlotUpdated;
        _linkedSlot = null;
        Setup(null, 0);
    }

    /// <summary>
    /// Set whether this slot is currently selected
    /// </summary>
    public void SetSelected(bool selected)
    {
        _bgSelected.gameObject.SetActive(selected);
    }

    private void OnLinkedSlotUpdated()
    {
        Assert.IsTrue(_linkedSlot != null, "OnLinkedSlotUpdated Invoked, yet no slot linked!");

        Setup(_linkedSlot.Item, _linkedSlot.Amount);
    }

    private void TryMoveItemOut()
    {

    }
}

using Blizzard.Inventory;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Zenject;
using System.Collections;
using Blizzard.Input;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Blizzard.UI.Core;
using Blizzard.Utilities.Logging;

namespace Blizzard.UI.Basic
{
    public class ItemDisplay : MonoBehaviour
    {
        public Button button;

        [Header("References")] 
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemCount;

        public void DisplayItem(ItemData item, int amount)
        {
	        _itemIcon.sprite = item.icon;
	        _itemCount.text = amount > 1 ? $"{amount}" : "";
        }

        public void SetIcon(Sprite icon)
        {
            _itemIcon.sprite = icon;
        }

        public void SetCountText(string text)
        {
            _itemCount.text = text;
        }
    }
}
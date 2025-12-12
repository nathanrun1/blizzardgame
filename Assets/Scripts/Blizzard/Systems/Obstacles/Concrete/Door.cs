using System;
using System.Collections.Generic;
using Blizzard.Environment;
using UnityEngine;
using Blizzard.Inventory;
using Blizzard.UI;
using Blizzard.UI.Core;
using Blizzard.Utilities;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Logging;
using Zenject;
using Random = UnityEngine.Random;

namespace Blizzard.Obstacles.Concrete
{
    public class WoodenDoor : Structure, IInteractable
    {
        [Inject] private UIService _uiService;
        [Inject] private EnvPrefabService _envPrefabService;
        
        [Header("References")]
        [SerializeField] private Sprite _openSprite;
        [SerializeField] private Sprite _closedSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _doorSpriteRenderer;
        
        public string PrimaryInteractText { get; private set; } = "Open";
        public bool PrimaryInteractReady => true;

        private bool _isOpen = false;

        private void OnEnable()
        {
            SetOpen(false);
        }

        public void OnPrimaryInteract()
        {
            SetOpen(!_isOpen);
        }

        private void SetOpen(bool open)
        {
            _doorSpriteRenderer.sprite = open ? _openSprite : _closedSprite;
            PrimaryInteractText = open ? "Close" : "Open";
            _collider.isTrigger = open;
            _isOpen = open;
        }
    }
}

// "bro i could eat this forever"

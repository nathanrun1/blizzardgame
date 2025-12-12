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
    public class Chest : Structure, IInteractable
    {
        [Inject] private UIService _uiService;
        [Inject] private EnvPrefabService _envPrefabService;
        
        public string PrimaryInteractText => "Open";
        public bool PrimaryInteractReady => true;

        [Header("Config")] 
        [SerializeField] private int _slotCount;
        /// <summary>
        /// Probability that any given item will drop when the chest is destroyed.
        /// </summary>
        [SerializeField] private float _itemPreserveChance;

        private readonly List<InventorySlot> _slots = new();
        
        private void OnEnable()
        {
            InitSlots();
        }

        public void OnPrimaryInteract()
        {
            _uiService.InitUI(UIID.Chest, new ChestUI.Args
            {
                slots = _slots
            });
        }

        protected override void OnDeath(DamageFlags damageFlags, Vector3 sourcePosition)
        {
            DropContents();
            base.OnDeath(damageFlags, sourcePosition);
        }


        /// <summary>
        /// Drops contents of chest (i.e. after the chest is destroyed), with each item having
        /// a set probability of dropping (rather than being destroyed with the chest)
        /// </summary>
        private void DropContents()
        {
            BLog.Log("Dropping contents");
            foreach (InventorySlot slot in _slots)
            {
                BLog.Log($"Slot with contents: {slot.Item} x{slot.Amount}");
                int amntToKeep = RandomAssistant.GenerateBinomial(slot.Amount, _itemPreserveChance);
                BLog.Log($"Keeping {amntToKeep}");
                if (amntToKeep < 1) continue;
                var dropObj = _envPrefabService.InstantiatePrefab("item_drop").GetComponent<ItemDrop>();
                dropObj.transform.position = transform.position +
                                             new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f), 0);
                dropObj.Setup(new ItemAmountPair
                {
                    item = slot.Item,
                    amount = amntToKeep
                });
                BLog.Log(dropObj);
            }
        }
        
        /// <summary>
        /// Initializes 'count' InventorySlot instances for storage within the chest
        /// </summary>
        private void InitSlots()
        {
            _slots.Clear();
            for (int i = 0; i < _slotCount; ++i)
            {
                _slots.Add(new InventorySlot());
            }
        }
    }
}

// "piperr"

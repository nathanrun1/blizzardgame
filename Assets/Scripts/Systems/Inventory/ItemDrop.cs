using Blizzard.Inventory;
using Blizzard.UI;
using ModestTree;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Blizzard.Environment
{
    public class ItemDrop : MonoBehaviour
    {
        [Inject] InventoryService _inventoryService;
        [Inject] UIService _uiService;

        [Header("References")]
        [SerializeField] SpriteRenderer _itemIcon;
        /// <summary>
        /// By how much the rotation of the item drap can vary from default in degrees
        /// </summary>
        [Header("Config")]
        [SerializeField] float _randRotationRange;
        [Header("Runtime Config")]
        /// <summary>
        /// Enabled iff player can currently pick up this item drop
        /// </summary>
        [SerializeField] bool isActive = false;

        [SerializeField] private ItemAmountPair _drop;

        public void Setup(ItemAmountPair drop)
        {
            this._drop = drop;
            this.isActive = true;
            _itemIcon.sprite = drop.item.icon;

            transform.localRotation = Quaternion.Euler(
                transform.localEulerAngles.x, 
                transform.localEulerAngles.y, 
                transform.localEulerAngles.z + Random.Range(-_randRotationRange, _randRotationRange
                ));
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!isActive || !collider.gameObject.CompareTag("Player")) return; // Ensure GameObject has "Player" tag and that drop is active
            Assert.That(_drop.item != null, "Test item data is not set!");

            int amountAdded = _inventoryService.TryAddItem(_drop.item, _drop.amount, true);
            _drop.amount -= amountAdded;

            _uiService.ItemGain(_drop.item, amountAdded, gameObject.transform.position);
    
            if (_drop.amount == 0) DisableDrop(); // Only remove once entire drop has been collected.
        }

        private void DisableDrop()
        {
            isActive = false;
            Destroy(gameObject);
        }
    }
}

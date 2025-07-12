using Blizzard.Inventory;
using Blizzard.UI;
using ModestTree;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace Blizzard.Interaction
{
    public class ItemDrop : MonoBehaviour
    {
        [Header("Testing")]
        [SerializeField] ItemData _testItemData;
        [SerializeField] int _testItemAmount;

        [Inject] InventoryService _inventoryService;
        [Inject] UIService _uiService;

        /// <summary>
        /// Enabled iff player can currently pick up this item drop
        /// </summary>
        bool isActive = true;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log("Detected collision!");
            Debug.Log("Player tag?: " + collider.CompareTag("Player"));
            if (!isActive || !collider.gameObject.CompareTag("Player")) return; // Ensure GameObject has "Player" tag and that drop is active
            Assert.That(_testItemData != null, "Test item data is not set!");

            int amountAdded = _inventoryService.TryAddItem(_testItemData, _testItemAmount, true);
            _testItemAmount -= amountAdded;

            _uiService.ItemGain(_testItemData, amountAdded, gameObject.transform.position);
    
            if (_testItemAmount == 0) DisableDrop(); // Only remove once entire drop has been collected.
        }

        private void DisableDrop()
        {
            isActive = false;
            Destroy(gameObject);
        }
    }
}

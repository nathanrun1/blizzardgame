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

namespace Blizzard.UI.Inventory
{
    public class InventorySlotCtrl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button slotButton;

        [Header("References")] 
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemCount;
        [SerializeField] private Image _bgSelected;
        [SerializeField] private Image _itemPreviewIcon;
        
        // Item Dragging

        /// <summary>
        /// Item icon used to show an item being dragged from the slot
        /// </summary>
        [SerializeField] private Image _itemIconDrag;
        /// <summary>
        /// Item count used to show an item being dragged from the slot
        /// </summary>
        [SerializeField] private TextMeshProUGUI _itemCountDrag;
        /// <summary>
        /// Parent to drag-dedicated icon & count, to move them together
        /// </summary>
        [SerializeField] private RectTransform _dragParent; 

        [Inject] private InputService _inputService;
        [Inject] private UIService _uiService;

        private static InventorySlotCtrl _mouseOverSlot = null; // Tracks which UI slot mouse is hovering over, if any
        private InventorySlot _linkedSlot;
        /// <summary>
        /// Whether items can be moved into this slot from other slots that have moving out enabled.
        /// Only relevant if linked to an InventorySlot instance.
        /// </summary>
        private bool _moveInEnabled = false;
        /// <summary>
        /// Whether items can be moved out of this slot to other slots that have moving in enabled.
        /// Only relevant if linked to an InventorySlot instance.
        /// </summary>
        private bool _moveOutEnabled = false;

        private struct DragState
        {
            public readonly bool isDragging;
            public readonly int amount;

            public DragState(bool isDragging, int amount)
            {
                this.isDragging = isDragging;
                this.amount = amount;
            }
        }

        private DragState _dragState = new(false, 0); // Used to manage item dragging

        private void OnDestroy()
        {
            // Reset drag state and re-parent drag preview to this object so it is cleaned up.
            // Otherwise, if still parented to canvasTop, may persist after this slot is destroyed.
            _dragState = new DragState(false, 0);
            _dragParent.SetParent(transform);
        }

        // Track the inventory slot that the mouse is hovered over (for dragging interactions)
        // TODO: potential race conditions here
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_dragState.isDragging) return; // If dragging, ignore pointer on this slot.

            BLog.Log($"OnPointerEnter: {this}");
            _mouseOverSlot = this;
            _inputService.inputActions.Player.Fire.performed += OnInputFire;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_mouseOverSlot == this)
            {
                BLog.Log($"OnPointerExit: {this}");
                _mouseOverSlot = null;
            }

            _inputService.inputActions.Player.Fire.performed -= OnInputFire;
        }


        /// <summary>
        /// Setup this slot with given item and amount
        /// </summary>
        public void Setup(ItemData item, int amount, bool isPreview = false)
        {
            _dragState = new DragState(false, 0); // Stop dragging if applicable
            _dragParent.gameObject.SetActive(false);

            if (item && amount > 0)
            {
                // Slot not empty
                _itemIconDrag.sprite = item.icon; // Add correct icon to drag preview
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
        public void LinkedSetup(InventorySlot slot, bool moveInEnabled = false, bool moveOutEnabled = false)
        {
            if (_linkedSlot != null)
            {
                // Slot already linked, unlink it.
                _linkedSlot.OnUpdate -= OnLinkedSlotUpdated;
            }

            Setup(slot.Item, slot.Amount);

            _moveInEnabled = moveInEnabled;
            _moveOutEnabled = moveOutEnabled;

            _linkedSlot = slot;
            _linkedSlot.OnUpdate += OnLinkedSlotUpdated;
        }

        /// <summary>
        /// Unlinks this slot from its linked InventorySlot instance, if it has been linked.
        /// Resets whether item moving is enabled to false.
        /// </summary>
        /// <param name="setupEmpty">Whether to clear the UI for this slot (i.e. set it to appear empty)</param>
        public void Unlink(bool setupEmpty = true)
        {
            if (_linkedSlot == null)
            {
                BLog.LogWarning("[InventorySlotCtrl] Unlink() called, yet not linked!");
                return;
            }

            _linkedSlot.OnUpdate -= OnLinkedSlotUpdated;
            _linkedSlot = null;
            if (setupEmpty) Setup(null, 0);

            _moveInEnabled = false;
            _moveOutEnabled = false;
        }

        /// <summary>
        /// Set whether this slot is currently selected
        /// </summary>
        public void SetSelected(bool selected)
        {
            _bgSelected.gameObject.SetActive(selected);
        }

        /// <summary>
        /// Sets whether items can be moved into this UI inventory slot from other UI inventory slots with moving out enabled.
        /// Must be linked with an InventorySlot instance, otherwise this has no effect.
        /// </summary>
        public void SetMoveInEnabled(bool moveInEnabled)
        {
            if (_linkedSlot == null) return;
            _moveInEnabled = moveInEnabled;
        }

        /// <summary>
        /// Sets whether items can be moved out of this UI inventory slot to other UI inventory slots with moving in enabled.
        /// Must be linked with an InventorySlot instance, otherwise this has no effect.
        /// </summary>
        public void SetMoveOutEnabled(bool moveOutEnabled)
        {
            if (_linkedSlot == null) return;
            _moveOutEnabled = moveOutEnabled;
        }

        /// <summary>
        /// Invoked when the linked InventorySlot instance is updated
        /// </summary>
        private void OnLinkedSlotUpdated()
        {
            Assert.IsTrue(_linkedSlot != null, "OnLinkedSlotUpdated Invoked, yet no slot linked!");

            Setup(_linkedSlot.Item, _linkedSlot.Amount);
        }

        /// <summary>
        /// Attempts to move given amount of contained item out from this slot to another slot.
        /// Will move as many as possible, returns amount successfully moved.
        /// Will perform a swap if destination slot has different item.
        /// Has no effect if _moveOutEnabled is false, or destination._moveInEnabled is false.
        /// </summary>
        /// <returns>Amount moved out successfully.</returns>
        public int TryMoveItemsOut(InventorySlotCtrl destination, int amount)
        {
            BLog.Log($"Attempting move out, amount: {amount}");
            if (_linkedSlot == null || destination._linkedSlot == null) return 0; // Missing linked slot. Not allowed.
            if (!_moveOutEnabled) return 0; // This slot does not have moving out enabled. Not allowed.
            if (!destination._moveInEnabled) return 0; // Other slot does not have moving in enabled. Not allowed.
            if (!_linkedSlot.Item || _linkedSlot.Amount == 0 || amount == 0) return 0; // Nothing to move out

            if (destination._linkedSlot.Item && destination._linkedSlot.Item != _linkedSlot.Item)
            {
                // Different items, must swap.
                if (!(_moveInEnabled && destination._moveOutEnabled))
                    return 0; // Both must have both directions enabled for swap.
                if (amount != _linkedSlot.Amount) return 0; // Must move entire stack out to swap. Otherwise, no can do.

                // Perform swap.
                ItemAmountPair temp;
                temp.item = _linkedSlot.Item;
                temp.amount = _linkedSlot.Amount; // Move from here to temp
                _linkedSlot.Item = destination._linkedSlot.Item;
                _linkedSlot.Amount = destination._linkedSlot.Amount; // Move from dest to here
                destination._linkedSlot.Item = temp.item;
                destination._linkedSlot.Amount = temp.amount; // Move from temp to dest
                return amount; // Entire stack moved out
            }

            // Same items or other slot empty, move as much as possible
            int amountInDestination = !destination._linkedSlot.Item ? 0 : destination._linkedSlot.Amount;
            int amountToMove =
                Math.Min(_linkedSlot.Item.stackSize - amountInDestination,
                    amount); // Can move at most as much as there is space in other slot
            if (amountToMove < 0) return 0; // No space at all
            BLog.Log($"Can move {amountToMove} to other slot. Other slot has {amountInDestination}, stack size is {_linkedSlot.Item.stackSize}");
            if (amountToMove > _linkedSlot.Amount)
            {
                BLog.LogError("InventorySlotCtrl",
                    $"Attempted to move out more items than in slot!\n" +
                    $"amountToMove = {amountToMove}, _linkedSlot.Amount = {_linkedSlot.Amount}");
                return 0;
            }

            destination._linkedSlot.Amount += amountToMove;
            if (!destination._linkedSlot.Item)
                destination._linkedSlot.Item = _linkedSlot.Item; // Added item to empty slot
            _linkedSlot.Amount -= amountToMove;
            if (_linkedSlot.Amount == 0) _linkedSlot.Item = null; // None left in this slot

            return amountToMove; // Success!
        }

        /// <summary>
        /// Handles internal UI behavior for when this slot is "clicked" (fired) on (e.g. dragging)
        /// </summary>
        private void OnInputFire(InputAction.CallbackContext _)
        {
            if (_linkedSlot == null || _linkedSlot.Amount < 1 || _dragState.isDragging) return;
            // Linked to inventory slot & item in slot, dragging behaviour supported
            // Start item drag
            bool shiftPressed =
                _inputService.inputActions.Player.UIInteract1.IsPressed(); // Full stack if shift drag
            int amountToDrag = shiftPressed ? _linkedSlot.Amount : 1;
            _dragState = new DragState(true, amountToDrag);
            StartCoroutine(DragCoroutine());
        }

        private IEnumerator DragCoroutine()
        {
            // Drag preview
            _dragParent.gameObject.SetActive(true);
            _dragParent.SetParent(_uiService.CanvasTop);
            _itemCountDrag.text = _dragState.amount > 1 ? _dragState.amount.ToString() : "";

            // Adjust slot content temporarily (as if item is actually being "picked up")
            var amountRemaining = _linkedSlot.Amount - _dragState.amount;
            _itemCount.text = amountRemaining > 1 ? amountRemaining.ToString() : "";
            if (amountRemaining < 1) _itemIcon.enabled = false;

            while (_dragState.isDragging &&
                   _inputService.inputActions.Player.Fire.IsPressed()) // TODO: adapt this code, uses OG mouse shit
            {
                _dragParent.position = Mouse.current.position.ReadValue();
                yield return null;
            }

            // Reset slot content to as it was (will get updated on item transfer if necessary)
            _itemIcon.enabled = true;
            _itemCount.text = _linkedSlot.Amount > 1 ? _linkedSlot.Amount.ToString() : "";

            // Drop the item if still in dragging state (i.e. mouse was just lifted)
            if (_dragState.isDragging && _mouseOverSlot && _mouseOverSlot != this)
                // Mouse is over different slot, attempt item transfer!
                TryMoveItemsOut(_mouseOverSlot, _dragState.amount);
            _dragState = new DragState(false, 0); // Reset dragging state (if not already)
            _dragParent.gameObject.SetActive(false);
            _dragParent.SetParent(transform);
        }
    }
}
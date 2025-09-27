using Blizzard.Obstacles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace Blizzard
{
    /// <summary>
    /// Player input and input-related helper methods
    /// </summary>
    public class InputService
    {
        public PlayerInputActions inputActions;

        public InputService()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable(); // Enabled by default
            BindInteractionInputs();
        }

        private void BindInteractionInputs()
        {
            Debug.Log("Bound interaction inputs!");
            inputActions.Player.Interact1.performed += OnPrimaryInteractionInput;
            inputActions.Player.Interact2.performed += OnSecondaryInteractionInput;
        }

        private void OnPrimaryInteractionInput(InputAction.CallbackContext ctx)
        {
            Collider2D pointerOverCollider = InputAssistant.GetColliderUnderPointer();
            if (pointerOverCollider == null) return;

            IInteractable interactable = pointerOverCollider.GetComponent<IInteractable>();
            interactable?.OnPrimaryInteract();
        }

        private void OnSecondaryInteractionInput(InputAction.CallbackContext ctx)
        {
            // TODO
        }
    }
}

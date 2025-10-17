using Blizzard.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Blizzard.Utilities.Assistants;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Manages the grid of obstacles existing in the world
    /// </summary>
    public class InteractionService : IInitializable
    {
        [Inject] private InputService _inputService;

        private Camera _mainCamera;
        
        public InteractionService()
        {
            Debug.Log("InteractionSErvice constructor");
            BindInteractionInputs();
        }
        
        public void Initialize()
        {
            _mainCamera = Camera.main;
        }

        private void BindInteractionInputs()
        {
            Debug.Log("Bound interaction inputs!");
            _inputService.inputActions.Player.Interact1.performed += OnPrimaryInteractionInput;
            _inputService.inputActions.Player.Interact2.performed += OnSecondaryInteractionInput;
        }

        private void OnPrimaryInteractionInput(InputAction.CallbackContext ctx)
        {
            Debug.Log("[InteractionService] Primary Interaction detected!");
            Collider2D pointerOverCollider = InputAssistant.GetColliderUnderPointer(_mainCamera);
            Debug.Log($"[InteractionService] pointer is over collider:{pointerOverCollider}");
            IInteractable interactable = pointerOverCollider.GetComponent<IInteractable>();
            interactable?.OnPrimaryInteract();
        }

        private void OnSecondaryInteractionInput(InputAction.CallbackContext ctx)
        {
            // TODO
        }
    }
}

// "BEEP beep BEEP beep BEEP beep BEEP beep the power is out bro."

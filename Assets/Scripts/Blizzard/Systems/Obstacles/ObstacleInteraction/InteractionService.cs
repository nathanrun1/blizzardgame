using Blizzard.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.Logging;

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
            BLog.Log("InteractionSErvice constructor");
            BindInteractionInputs();
        }

        public void Initialize()
        {
            _mainCamera = Camera.main;
        }

        private void BindInteractionInputs()
        {
            BLog.Log("Bound interaction inputs!");
            _inputService.inputActions.Player.Interact1.performed += OnPrimaryInteractionInput;
            _inputService.inputActions.Player.Interact2.performed += OnSecondaryInteractionInput;
        }

        private void OnPrimaryInteractionInput(InputAction.CallbackContext ctx)
        {
            BLog.Log("[InteractionService] Primary Interaction detected!");
            var pointerOverCollider = InputAssistant.GetColliderUnderPointer(_mainCamera);
            BLog.Log($"[InteractionService] pointer is over collider:{pointerOverCollider}");
            var interactable = pointerOverCollider.GetComponent<IInteractable>();
            interactable?.OnPrimaryInteract();
        }

        private void OnSecondaryInteractionInput(InputAction.CallbackContext ctx)
        {
            // TODO
        }
    }
}

// "BEEP beep BEEP beep BEEP beep BEEP beep the power is out bro."
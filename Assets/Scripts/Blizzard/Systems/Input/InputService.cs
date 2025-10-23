using Blizzard.Obstacles;
using Blizzard.Utilities.Assistants;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Blizzard.Utilities.Logging;


namespace Blizzard.Input
{
    /// <summary>
    /// Player input and input-related helper methods
    /// </summary>
    public class InputService : IInitializable
    {
        public PlayerInputActions inputActions;

        private Camera _mainCamera;

        public InputService()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable(); // Enabled by default
            BindInteractionInputs();
        }

        public void Initialize()
        {
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Retrieves the current cached main camera
        /// </summary>
        /// <returns></returns>
        public Camera GetMainCamera()
        {
            return _mainCamera;
        }


        private void BindInteractionInputs()
        {
            BLog.Log("Bound interaction inputs!");
            inputActions.Player.Interact1.performed += OnPrimaryInteractionInput;
            inputActions.Player.Interact2.performed += OnSecondaryInteractionInput;
        }

        private void OnPrimaryInteractionInput(InputAction.CallbackContext ctx)
        {
            var pointerOverCollider = InputAssistant.GetColliderUnderPointer(_mainCamera);
            if (pointerOverCollider == null) return;

            var interactable = pointerOverCollider.GetComponent<IInteractable>();
            interactable?.OnPrimaryInteract();
        }

        private void OnSecondaryInteractionInput(InputAction.CallbackContext ctx)
        {
            // TODO
        }
    }
}
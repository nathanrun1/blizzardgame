using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.Temperature;
using Zenject;
using UnityEngine.InputSystem;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Manages the grid of obstacles existing in the world
    /// </summary>
    public class InteractionService
    {
        [Inject] private InputService _inputService;

        public InteractionService()
        {
            Debug.Log("InteractionSErvice constructor");
            BindInteractionInputs();
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
            Collider2D pointerOverCollider = InputAssistant.GetColliderUnderPointer();
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

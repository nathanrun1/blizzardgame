using System;
using Blizzard.Input;
using Blizzard.UI;
using Blizzard.UI.Core;
using Blizzard.Utilities.Logging;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// Handles underlying interaction system. Should be attached to any object with a script
    /// implementing IInteractable.
    /// </summary>
    public class InteractionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Inject] private InputService _inputService;
        [Inject] private UIService _uiService;
        
        /// <summary>
        /// IInteractable script attached to the same object
        /// </summary>
        [SerializeField] private MonoBehaviour _interactable;

        /// <summary>
        /// Current target interactable, i.e. the one that the pointer is currently hovered over, if any.
        /// </summary>
        private static IInteractable _curTargetInteractable = null;
        /// <summary>
        /// Whether interaction system (static) has been initialized
        /// </summary>
        private static bool _interactionInitialized = false;
        
        [Inject]
        private void Init(InputService inputService, UIService uiService)
        {
            _inputService = inputService;
            _uiService = uiService;
            
            if (!_interactionInitialized) InitInteraction(_inputService);  // Init static functionality
            Assert.IsTrue(_interactable is IInteractable, "Interactable script must inherit from IInteractable!");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            BLog.Log("Interactable pointer enter");
            _curTargetInteractable = _interactable as IInteractable;
            _uiService.InitUI("interact_info", new InteractInfoUI.Args(
                interactable: _interactable as IInteractable, 
                interactablePosition: transform.position));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            BLog.Log("Interactable pointer exit");
            if (_curTargetInteractable != (_interactable as IInteractable)) return; // Has already been reassigned to other interactable
            _curTargetInteractable = null;
            _uiService.CloseUI("interact_info");
        }

        
        private static void InitInteraction(InputService inputService)
        {
            // Bind input to interaction logic
            inputService.inputActions.Player.Interact1.performed += OnPrimaryInteractionInput;
            inputService.inputActions.Player.Interact2.performed += OnSecondaryInteractionInput;
            _interactionInitialized = true;
        }
        
        private static void OnPrimaryInteractionInput(InputAction.CallbackContext ctx)
        {
            _curTargetInteractable?.OnPrimaryInteract();
        }
        private static void OnSecondaryInteractionInput(InputAction.CallbackContext ctx)
        {
            (_curTargetInteractable as ISecondaryInteractable)?.OnSecondaryInteract();
        }
    }
}
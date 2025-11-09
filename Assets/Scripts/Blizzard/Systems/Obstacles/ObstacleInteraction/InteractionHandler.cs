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
            
            if (!_interactionInitialized) InitInteraction(_inputService, uiService);  // Init static functionality
            Assert.IsTrue(_interactable is IInteractable, "Interactable script must inherit from IInteractable!");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            BLog.Log("Interactable pointer enter");
            _curTargetInteractable = _interactable as IInteractable;
            if (!_curTargetInteractable!.PrimaryInteractReady) return;  // Interaction must be ready
            _uiService.InitUI(UIID.InteractInfo, new InteractInfoUI.Args(
                interactable: _interactable as IInteractable, 
                interactablePosition: transform.position));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            BLog.Log("Interactable pointer exit");
            if (_curTargetInteractable != (_interactable as IInteractable)) return; // Has already been reassigned to other interactable
            _curTargetInteractable = null;
            _uiService.CloseUI(UIID.InteractInfo);
        }

        
        private static void InitInteraction(InputService inputService, UIService uiService)
        {
            // Bind input to interaction logic
            inputService.inputActions.Player.Interact1.performed += (ctx) =>
            {
                OnPrimaryInteractionInput(uiService);
            };
            inputService.inputActions.Player.Interact2.performed += (ctx) =>
            {
                OnSecondaryInteractionInput(uiService);
            };
            _interactionInitialized = true;
        }
        
        private static void OnPrimaryInteractionInput(UIService uiService)
        {
            if (_curTargetInteractable is { PrimaryInteractReady: true })
                _curTargetInteractable.OnPrimaryInteract();
            uiService.CloseUI(UIID.InteractInfo); // Reset UI in case interaction no longer ready 
        }
        private static void OnSecondaryInteractionInput(UIService uiService)
        {
            if ((_curTargetInteractable as ISecondaryInteractable) is { SecondaryInteractReady: true})
                (_curTargetInteractable as ISecondaryInteractable)!.OnSecondaryInteract();
            uiService.CloseUI(UIID.InteractInfo);
        }
    }
}
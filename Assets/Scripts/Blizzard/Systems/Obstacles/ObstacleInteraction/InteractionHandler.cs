using System;
using Blizzard.Input;
using Blizzard.Player;
using Blizzard.UI;
using Blizzard.UI.Core;
using Blizzard.Utilities.Logging;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
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
        [Inject] private PlayerService _playerService;

        /// <summary>
        /// IInteractable script attached to the same object
        /// </summary>
        [FormerlySerializedAs("_interactable")]
        [Header("References")]
        [SerializeField] private MonoBehaviour _interactableScript;
        private IInteractable _interactable => _interactableScript as IInteractable;
        /// <summary>
        /// Maximum interaction distance
        /// </summary>
        [Header("Config")]
        [SerializeField] private float _maxDistance = float.MaxValue;

        /// <summary>
        /// Current target interactable's handler, i.e. the one that the pointer is currently hovered over, if any.
        /// </summary>
        private static InteractionHandler _curTargetHandler = null;
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
            Assert.IsTrue(_interactableScript is IInteractable, "Interactable script must inherit from IInteractable!");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _curTargetHandler = this;
            if (!(_interactable.PrimaryInteractReady && CanInteract())) return;
            _uiService.InitUI(UIID.InteractInfo, new InteractInfoUI.Args(
                interactable: _interactable as IInteractable, 
                interactablePosition: transform.position));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_curTargetHandler != this) return; // Has already been reassigned to other handler
            _curTargetHandler = null;
            _uiService.CloseUI(UIID.InteractInfo);
        }

        /// <summary>
        /// Determines whether interaction with this interactable is currently allowed based on
        /// interaction handler configuration.
        /// </summary>
        /// <returns></returns>
        private bool CanInteract()
        {
            return (_playerService.PlayerPosition - (Vector2)transform.position).magnitude <= _maxDistance;
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
            if (_curTargetHandler != null && _curTargetHandler._interactable.PrimaryInteractReady && _curTargetHandler.CanInteract())
                _curTargetHandler._interactable.OnPrimaryInteract();
            uiService.CloseUI(UIID.InteractInfo);
        }
        private static void OnSecondaryInteractionInput(UIService uiService)
        {
            if (_curTargetHandler != null && (_curTargetHandler._interactable as ISecondaryInteractable) is { SecondaryInteractReady: true} 
                && _curTargetHandler.CanInteract())
                (_curTargetHandler._interactable as ISecondaryInteractable)!.OnSecondaryInteract();
            uiService.CloseUI(UIID.InteractInfo);
        }
    }
}
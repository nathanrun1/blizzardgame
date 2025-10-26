// using UnityEngine;
// using UnityEngine.InputSystem;
// using Zenject;
// using Blizzard.Input;
// using Blizzard.UI;
// using Blizzard.UI.Core;
// using Blizzard.Utilities.Assistants;
// using Blizzard.Utilities.Logging;
//
// namespace Blizzard.Obstacles
// {
//     /// <summary>
//     /// Manages the grid of obstacles existing in the world
//     /// </summary>
//     public class InteractionService : IInitializable//, ITickable
//     {
//         [Inject] private InputService _inputService;
//         [Inject] private UIService _uiService;
//
//         private Camera _mainCamera;
//
//         /// <summary>
//         /// Interactable that the pointer is currently hovered over
//         /// </summary>
//         private IInteractable _hoveredInteractable;
//
//         public void Initialize()
//         {
//             BindInteractionInputs();
//         }
//
//         // public void Tick()
//         // {
//         //     Collider2D pointerOverCollider = InputAssistant.GetColliderUnderPointer(_inputService.GetMainCamera());
//         //     if (!pointerOverCollider) return;
//         //
//         //     
//         //     if (pointerOverCollider.TryGetComponent<IInteractable>(out var interactable))
//         //     {
//         //         _uiService.InitUI("interact_info", new InteractInfoUI.Args(interactable));
//         //         _interactionInfoOpen = true;
//         //         if (_inputService.inputActions.Player.Interact1.WasPressedThisFrame())
//         //         {
//         //             interactable.OnPrimaryInteract();
//         //         }
//         //
//         //         if (interactable is ISecondaryInteractable secondaryInteractable 
//         //             && _inputService.inputActions.Player.Interact2.WasPressedThisFrame())
//         //         {
//         //             secondaryInteractable.OnSecondaryInteract();
//         //         } 
//         //     }
//         //     else if (_interactionInfoOpen)
//         //     {
//         //         _uiService.CloseUI("interact_info");
//         //         _interactionInfoOpen = false;
//         //     }
//         // }
//
//         
//         private void BindInteractionInputs()
//         {
//             _inputService.inputActions.Player.Interact1.performed += OnPrimaryInteractionInput;
//             _inputService.inputActions.Player.Interact2.performed += OnSecondaryInteractionInput;
//             BLog.Log("Bound interaction inputs!");
//         }
//
//         private void OnPrimaryInteractionInput(InputAction.CallbackContext ctx)
//         {
//             Collider2D pointerOverCollider = InputAssistant.GetColliderUnderPointer(_inputService.GetMainCamera());
//             if (pointerOverCollider == null) return;
//
//             var interactable = pointerOverCollider.GetComponent<IInteractable>();
//             interactable?.OnPrimaryInteract();
//         }
//
//         private void OnSecondaryInteractionInput(InputAction.CallbackContext ctx)
//         {
//             Collider2D pointerOverCollider = InputAssistant.GetColliderUnderPointer(_inputService.GetMainCamera());
//             if (pointerOverCollider == null) return;
//
//             var interactable = pointerOverCollider.GetComponent<ISecondaryInteractable>();
//             interactable?.OnSecondaryInteract();
//         }
//     }
// }
//
// // "BEEP beep BEEP beep BEEP beep BEEP beep the power is out bro."
using System;
using Blizzard.Input;
using TMPro;
using UnityEngine;
using Blizzard.Obstacles;
using Blizzard.UI.Core;
using UnityEngine.InputSystem;
using Zenject;


namespace Blizzard.UI
{
    /// <summary>
    /// UI that briefly appears on the screen to showcase the player collecting an item
    /// </summary>
    public class InteractInfoUI : UIBase
    {
        public struct Args
        {
            public readonly IInteractable interactable;
            /// <summary>
            /// Position of interactable in world space
            /// </summary>
            public readonly Vector3 interactablePosition;

            public Args(IInteractable interactable, Vector3 interactablePosition)
            {
                this.interactable = interactable;
                this.interactablePosition = interactablePosition;
            }
        }

        [Header("GameObject References")] 
        [SerializeField] private TextMeshProUGUI _text;

        /// <summary>
        /// Offset of interact info UI position from mouse position (pixel coords)
        /// </summary>
        [Header("Config")] 
        [SerializeField] private Vector2 _mousePositionOffset;
        [Inject] private InputService _inputService;

        public override void Setup(object args)
        {
            Args interactInfoArgs;
            try
            {
                interactInfoArgs = (Args)args;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Incorrect argument type given!");
            }
            
            SetupText(interactInfoArgs.interactable);
            //transform.position = _inputService.GetMainCamera().WorldToScreenPoint(interactInfoArgs.interactablePosition); // To interactable pos
            MoveToMousePos();
        }

        private void Update()
        {
            MoveToMousePos();
        }

        /// <summary>
        /// Moves the UI to the mouse's position (with offset)
        /// </summary>
        private void MoveToMousePos()
        {
            // To mouse pos
            transform.position = Mouse.current.position.ReadValue() + _mousePositionOffset;
        }

        /// <summary>
        /// Sets the UI text according to interaction info
        /// </summary>
        /// <param name="interactable"></param>
        private void SetupText(IInteractable interactable)
        {
            // String representation of bindings
            string interact1Binding = _inputService.inputActions.Player.Interact1.GetBindingDisplayString();
            string interact2Binding = _inputService.inputActions.Player.Interact2.GetBindingDisplayString();

            string interact1 = $"[{interact1Binding}] {interactable.PrimaryInteractText}";
            ISecondaryInteractable secondaryInteractable = interactable as ISecondaryInteractable;;
            string interact2 = secondaryInteractable != null
                ? $"\n[{interact2Binding}] {secondaryInteractable.SecondaryInteractText}" : "";

            _text.text = interact1 + interact2;
        }
    }
}
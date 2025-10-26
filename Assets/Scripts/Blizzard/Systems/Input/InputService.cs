using Blizzard.Obstacles;
using Blizzard.Utilities.Assistants;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Blizzard.Utilities.Logging;
using UnityEngine.PlayerLoop;


namespace Blizzard.Input
{
    /// <summary>
    /// Player input and input-related helper methods
    /// </summary>
    public class InputService : IInitializable
    {
        public readonly PlayerInputActions inputActions;

        private Camera _mainCamera;

        public InputService()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable(); // Enabled by default
            inputActions.UI.Enable();     // ^^
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
    }
}
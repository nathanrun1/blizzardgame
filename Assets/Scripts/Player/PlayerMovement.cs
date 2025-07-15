using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Blizzard
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] public GameObject playerObj;

        [SerializeField] private Rigidbody2D _rigidBody;
        [Tooltip("Speed in units/second")]
        [SerializeField] private float _walkSpeed;
        
        [Inject] private InputService _inputService;

        private Vector2 _movementVector = new Vector2(0, 0);


        private void UpdateMovementVector()
        {
            Vector2 rawInput = _inputService.inputActions.Player.Move.ReadValue<Vector2>();
            _movementVector = rawInput.magnitude > 0.95f ? rawInput : Vector2.zero;
        }

        private void UpdatePlayerVelocity()
        {
            _rigidBody.MovePosition(_rigidBody.position + _movementVector * _walkSpeed * Time.fixedDeltaTime);
        //    _rigidBody.AddForce(_movementVector - _rigidBody.linearVelocity, ForceMode.VelocityChange);
        //    _rigidBody.linearVelocity = _movementVector * _walkSpeed;
        }

        private void PlayerLookAtMouse()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 diff = mousePos - new Vector2(Screen.width / 2, Screen.height / 2);
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            playerObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }

        private void Update()
        {
            PlayerLookAtMouse();
        }

        private void FixedUpdate()
        {
            UpdateMovementVector();
            UpdatePlayerVelocity();
        }
    }
}

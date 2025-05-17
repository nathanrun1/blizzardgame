using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Blizzard
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidBody;
        [SerializeField] private GameObject _playerObj;
        [Tooltip("Speed in units/second")]
        [SerializeField] private float _walkSpeed;
        [SerializeField] private PlayerInput _playerInput;

        private Vector2 _movementVector = new Vector2(0, 0);

        private void OnMoveInput(InputAction.CallbackContext context)
        {
            Vector2 rawInput = context.ReadValue<Vector2>();
            //Debug.Log(rawInput);
            _movementVector = rawInput.magnitude > 0.95f ? rawInput : Vector2.zero; // TODO: Fix so that input doesn't do weird shit, for now just ensure magnitude is 1 since when it drifts it's not normalized
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
            _playerObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }

        private void AddInputListener()
        {
            Action addInputListener = () =>
            {
                _playerInput.inputActions.Player.Move.performed += OnMoveInput;
                _playerInput.inputActions.Player.Move.canceled += OnMoveInput;
            };

            if (!_playerInput.inputReady) _playerInput.OnInputReady += addInputListener;
            else addInputListener.Invoke();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            AddInputListener();
        }

        // Update is called once per frame
        private void Update()
        {
            PlayerLookAtMouse();
        }

        private void FixedUpdate()
        {
            UpdatePlayerVelocity();
        }
    }
}

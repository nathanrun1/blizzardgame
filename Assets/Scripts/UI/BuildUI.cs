using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Zenject;
using Blizzard.Obstacles;
using Blizzard.Grid;
using Blizzard.Building;
using Blizzard.Utilities;
using System.ComponentModel;
using UnityEngine.UIElements;


namespace Blizzard
{   
    public class BuildUI : MonoBehaviour
    {
        private interface IBuildState : IState
        {
            public void OnInputPlaceBuilding(Vector3 position) { }
            public void OnInputToggleBuild() { }
        }

        private class BuildStateContext
        {
            public StateMachine stateMachine;

            public ObstacleGridService obstacleGridService;
            // Testing:
            public BuildingData testBuilding;
            public TextMeshProUGUI buildModeTest;
        }

        private class SelectState : IBuildState
        {
            private BuildStateContext _stateContext;

            public SelectState(BuildStateContext stateContext)
            {
                _stateContext = stateContext;
            }

            public void Enter() 
            {
                _stateContext.buildModeTest.text = "BUILDING MODE: OFF"; // TEMP hardcoded
            }
            public void Exit() { }

            public void Update() { }

            public void OnInputPlaceBuilding(Vector3 _) { }
            public void OnInputToggleBuild() 
            {
                // Enter building mode
                _stateContext.stateMachine.ChangeState(new BuildState(_stateContext, _stateContext.testBuilding)); // TEMP: use test building (should manage building selection somewhere)
            }
        }

        private class BuildState : IBuildState
        {
            private BuildStateContext _stateContext;
            /// <summary>
            /// Currently selected building
            /// </summary>
            private BuildingData _buildingData;
            /// <summary>
            /// 
            /// </summary>
            private GameObject _buildingPreview;

            public BuildState(BuildStateContext stateContext, BuildingData buildingData)
            {
                _stateContext = stateContext;
                _buildingData = buildingData;
            }
            public void Enter()
            {
                Debug.Log($"Building a {_buildingData.displayName}");
                _stateContext.buildModeTest.text = "BUILDING MODE: ON"; // TEMP hardcoded

                _buildingPreview = _buildingData.obstacleData.obstaclePrefab.CreatePreview();
            }

            public void Exit() 
            {
                MonoBehaviour.Destroy(_buildingPreview); // TEMP, TOOD: Change to pooling
            }

            public void Update()
            {
                // Display preview of building at mouse pos
                Vector2Int mouseGridPosition = _stateContext.obstacleGridService.Grid.WorldToCellPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                _buildingPreview.transform.position = _stateContext.obstacleGridService.Grid.CellToWorldPosCenter(mouseGridPosition);
            }

            public void OnInputPlaceBuilding(Vector3 position)
            {
                // Build the building!
                Vector2Int mouseGridPosition = _stateContext.obstacleGridService.Grid.WorldToCellPos(Camera.main.ScreenToWorldPoint(position));
                Debug.Log($"Placing at {mouseGridPosition}");
                _stateContext.obstacleGridService.PlaceObstacleAt(mouseGridPosition, _buildingData.obstacleData);
            }

            public void OnInputToggleBuild()
            {
                // Exit building mode
                _stateContext.stateMachine.ChangeState(new SelectState(_stateContext));
            }
        }


        [Inject] private ObstacleGridService _obstacleGridService;
        [Inject] private InputService _inputService;

        [Header("Config")]
        [SerializeField] private float _updateDelay = 0.2f;

        [Header("Testing")]
        [SerializeField] private BuildingData _testBuilding;
        [SerializeField] private TextMeshProUGUI _buildModeTest;
        [SerializeField] private UnityEngine.UI.Button _buildToggleButton;

        private StateMachine _stateMachine = new StateMachine();
        private BuildStateContext _stateContext = new BuildStateContext();

        private float _updateDelayTimer = 0f;

        private void OnInputFire(InputAction.CallbackContext _)
        {
            IBuildState state = _stateMachine.currentState as IBuildState;
            if (!InputAssistant.IsPointerOverUIElement()) state.OnInputPlaceBuilding(Input.mousePosition);
        }

        private void OnInputToggleBuild()
        {
            IBuildState state = _stateMachine.currentState as IBuildState;
            state.OnInputToggleBuild();
        }
            
        private void Start()
        {
            _stateContext.stateMachine = _stateMachine;
            _stateContext.obstacleGridService = _obstacleGridService;
            _stateContext.testBuilding = _testBuilding;
            _stateContext.buildModeTest = _buildModeTest;

            _stateMachine.ChangeState(new SelectState(_stateContext));

            _inputService.inputActions.Player.Fire.performed += OnInputFire;
            _buildToggleButton.onClick.AddListener(OnInputToggleBuild);
        }

        private void Update()
        {
            _updateDelayTimer += Time.unscaledDeltaTime;
            if (_updateDelayTimer > _updateDelay)
            {
                _updateDelayTimer -= _updateDelay;
                _stateMachine.Update();
            }
        }
    }
}

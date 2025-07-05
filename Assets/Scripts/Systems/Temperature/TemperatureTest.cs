using Blizzard.Grid;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Blizzard.Temperature
{
    /// <summary>
    /// Class used to test and display the heat diffusion simulation from TemperatureService
    /// </summary>
    public class TemperatureTest : MonoBehaviour
    {
        [SerializeField] private ComputeShader _heatDiffusionShader;
        [SerializeField] private InputService _playerInput;

        [Header("Simulation Params")]
        [SerializeField] private int _windowWidth = 16;
        [SerializeField] private int _windowHeight = 16;
        [SerializeField, Range(0.5f, 20f)] private float _timeScale = 1.0f;

        [Header("Real time modifications (SPACEBAR to apply them at selected square)")]
        [SerializeField] private float setTemperature = 10.0f;
        [SerializeField] private float setInsulation = 0.0f;
        [SerializeField] private float setSourceHeat = 0.0f;
        private Vector2Int _selectedCell = new(0, 0);

        [Header("Heatmap display")]
        [SerializeField] private Image _heatmap;
        [SerializeField] private TextMeshProUGUI _temperatureText;

        private TemperatureService _temperatureService;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SetupTemperatureService();
            BindInputs();
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log(_temperatureService);
            _temperatureService.DoHeatDiffusionStep(Time.deltaTime * _timeScale);
            _temperatureService.ComputeHeatmap();
            _temperatureText.text = $"{_temperatureService.Grid.GetAt(_selectedCell).temperature}°"; // Display temperature at selected cell
            UpdateHeatmap();
        }

        private void SetupTemperatureService()
        {
            var mainGrid = new DenseWorldGrid<TemperatureCell>(1f, 1f, 1000, 1000); // Arbitrary main grid dimensions
            mainGrid.Initialize(new TemperatureCell
            {
                temperature = 10, // Set initial temperature of all cells to 10
                insulation = 0,
                heat = 0
            });
            _temperatureService = new TemperatureService(
                    mainGrid, 
                    new BasicDenseGrid<TemperatureCell>(_windowWidth, _windowHeight),
                    _heatDiffusionShader
                );
            Debug.Log(_temperatureService);
        }

        private void UpdateHeatmap()
        {
            _heatmap.material.SetTexture("_MainTex", _temperatureService.HeatmapTexture);
        }

        private void BindInputs()
        {
            // Arrow keys to move selected square
            _playerInput.inputActions.Player.Move.performed += (ctx) =>
            {
                if (!ctx.action.WasPressedThisFrame()) return;
                Vector2 input = ctx.ReadValue<Vector2>();
                OnMoveSelectedSquare(input);
            };

            // Spacebar to apply values set in inspector
            _playerInput.inputActions.Player.Space.performed += (ctx) =>
            {
                if (!ctx.action.WasPressedThisFrame()) return;
                OnSetSelectedSquare(new TemperatureCell
                {
                    temperature = setTemperature,
                    insulation = setInsulation,
                    heat = setSourceHeat
                });
            };
        }

        private void OnMoveSelectedSquare(Vector2 input)
        {
            _selectedCell += new Vector2Int((int)input.x, (int)input.y);

            _heatDiffusionShader.SetInt("selectedX", _selectedCell.x);
            _heatDiffusionShader.SetInt("selectedY", _selectedCell.y);
        }

        private void OnSetSelectedSquare(TemperatureCell value)
        {
            _temperatureService.Grid.SetAt(_selectedCell, value);
        }
    }
}

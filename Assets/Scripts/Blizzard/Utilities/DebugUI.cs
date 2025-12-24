using System;
using Blizzard.Input;
using Blizzard.Player;
using Blizzard.Temperature;
using Blizzard.UI.Core;
using Blizzard.Utilities.Assistants;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;


namespace Blizzard.Utilities
{
    public class DebugUI : UIBase
    {
        [Inject] private PlayerService _playerService;
        [Inject] private PlayerTemperatureService _playerTemperatureService;
        [Inject] private InputService _inputService;
        [Inject] private TemperatureService _temperatureService;
        
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _fpsUpdateDelay = .5f;

        private float _lastUpdateTime;
        private int _framesSinceLastUpdate = 0;
        
        // Displayed info
        private int _fps;

        private void Start()
        {
            _lastUpdateTime = Time.unscaledTime;
        }

        private void Update()
        {
            UpdateFps();
            UpdateDisplayedInfo();
        }

        private void UpdateDisplayedInfo()
        {
            // FPS: 100
            // Player
            // Area Temp: 12, Body Temp: 10, Body Insul: 0.75
            // Pos: (1.045, -3.044), Grid Pos: (4, -12)
            // Mouse
            // Pos: (1.443, 4.444), Grid Pos: (5, 17)
            // Temp: 10, Insul: 0.5, Heat: 0
            Vector2 mousePos = GetMousePosition();
            TemperatureCell mousePosTempData = _temperatureService.Grid.GetAt(mousePos.ToCellPos());
            _text.text = $"FPS: {_fps}\n" +
                         $"\nPlayer\n" +
                         $"Area Temp: {Math.Round(_temperatureService.GetTemperatureAtWorldPos(_playerService.PlayerPosition), 2)}, Body Temp: {Math.Round(_playerTemperatureService.BodyTemperature, 2)}, Body Insul: {Math.Round(_playerTemperatureService.BodyInsulation, 2)}\n" +
                         $"Pos: {_playerService.PlayerPosition}, Grid Pos: {_playerService.PlayerPosition.ToCellPos()}\n" +
                         $"\nMouse\n" +
                         $"Pos: {mousePos}, Grid Pos: {mousePos.ToCellPos()}\n" +
                         $"Temp: {Math.Round(mousePosTempData.temperature, 2)}, Insul: {mousePosTempData.insulation}\nHeat: {mousePosTempData.heat}, Ambient: {mousePosTempData.ambient}";
        }

        private void UpdateFps()
        {
            _framesSinceLastUpdate++;
            if (Time.unscaledTime - _lastUpdateTime > _fpsUpdateDelay)
            {
                _fps = Mathf.RoundToInt(_framesSinceLastUpdate / _fpsUpdateDelay);
                _framesSinceLastUpdate = 0;
                _lastUpdateTime = Time.unscaledTime;
            }
        }

        private Vector2 GetMousePosition()
        {
            return _inputService.GetMainCamera().ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        }

        public override void Setup(object args)
        {
        }
    }
}
using Blizzard.Config;
using Blizzard.Grid;
using Blizzard.Temperature;
using UnityEngine;
using Zenject;

namespace Blizzard.Player
{
    /// <summary>
    /// Manages player data and interactions
    /// </summary>
    public class PlayerTemperatureService : IInitializable, IFixedTickable
    {
        [Inject] private PlayerService _playerService;
        [Inject] private TemperatureService _temperatureService;
        [Inject] private PlayerTemperatureConfig _config;
        
        public float BodyTemperature { get; private set; }
        public float BodyInsulation { get; set; }

        
        private float _bodyHeat;

        public void Initialize()
        {
            // Set initial body temperature
            BodyTemperature = _config.neutralBodyTemperature;
            
            // Calculate body heat based on config:
            // At neutral external, body heat must equal -(temperature change) for equilibrium
            _bodyHeat = _config.bodyTemperatureChangeRate *
                        (_config.neutralBodyTemperature - _config.neutralExternalTemperature);
        }

        public void FixedTick()
        {
            UpdateBodyTemperature(Time.fixedDeltaTime);
            // Update temperature service's window offset
            _temperatureService.WindowOffset =
                _temperatureService.Grid.WorldToCellPos(_playerService.PlayerPosition) -
                new Vector2Int(16, 16); // TEMP: place player at center of window
        }
        
        private void UpdateBodyTemperature(float deltaTime)
        {
            float tempDelta = deltaTime * (_bodyHeat + (1 - BodyInsulation) * _config.bodyTemperatureChangeRate *
                (GetExternalTemperature() - BodyTemperature));
            BodyTemperature += tempDelta;
        }
        
        private float GetExternalTemperature()
        {
            return _temperatureService.GetTemperatureAtWorldPos(_playerService.PlayerPosition);
        }
    }
}
using Blizzard.Config;
using Blizzard.Grid;
using Blizzard.Temperature;
using Blizzard.Utilities.DataTypes;
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
        
        /// <summary>
        /// Player's body temperature
        /// </summary>
        public float BodyTemperature { get; private set; }
        /// <summary>
        /// Player's body insulation. Body temperature change as affected by external temperature
        /// is multiplied by (1 - this value)
        /// </summary>
        public float BodyInsulation { get; set; }

        
        private float _bodyHeat;
        private float _temperatureDamageClock;

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

            _temperatureDamageClock += Time.fixedDeltaTime;
            if (_temperatureDamageClock > _config.temperatureDamageDelay)
            {
                _temperatureDamageClock -= _config.temperatureDamageDelay;
                InflictTemperatureDamage();
            }
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

        private void InflictTemperatureDamage()
        {
            int damageToInflict = 0;
            for (int i = 0; i < _config.temperatureDamageLevels.Length; ++i)
            {
                if (BodyTemperature >= _config.temperatureDamageLevels[i].threshold) break;
                damageToInflict = Mathf.CeilToInt(
                    _config.temperatureDamageLevels[i].damagePerSecond * _config.temperatureDamageDelay);
            }

            if (damageToInflict == 0) return;
            _playerService.DamagePlayer(damageToInflict, DamageFlags.Cold);
        }
    }
}
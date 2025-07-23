using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Zenject;
using Blizzard.Temperature;
using Blizzard.Grid;


namespace Blizzard
{
    public class PlayerTemperature : MonoBehaviour
    {
        [Inject] TemperatureService _temperatureService;
        
        [Header("Config")]
        [SerializeField] PlayerTemperatureConfig _config;

        [Header("Testing")]
        [SerializeField, Range(0f, 1f)] float _bodyInsulation = 0f;
        [SerializeField] float _timeScale = 1f;
        [SerializeField] bool _useTestExternalTemp = false;
        [SerializeField] float _TESTExternalTemperature;

        public float BodyTemperature { get; set; }

        private float _bodyHeat;

        private void Awake()
        {
            BodyTemperature = _config.neutralBodyTemperature;
            // At neutral external, body heat must equal -(temperature change) for equilibrium
            _bodyHeat = _config.bodyTemperatureChangeRate * (_config.neutralBodyTemperature - _config.neutralExternalTemperature); 
        }

        private void Update()
        {
            UpdateBodyTemperature(Time.deltaTime * _timeScale);
            // Update temperature service's window offset
            _temperatureService.WindowOffset = _temperatureService.Grid.WorldToCellPos(transform.position) - new Vector2Int(16, 16); // TEMP: place player at center of window
        }

        private void UpdateBodyTemperature(float deltaTime)
        {
            //float internalEquiv = _internalExternalEquivRatio * (_temperatureService.GetTemperatureAtWorldPos(transform.position) - _neutralExternalTemperature)
            //    + _neutralBodyTemperature;
            //Debug.Log(internalEquiv);
            //float tempDelta = internalEquiv - BodyTemperature; // Difference btwn current body temperature and stable body temperature
            //BodyTemperature += (1 - _bodyInsulation) * deltaTime * _bodyTemperatureChangeRate * tempDelta;

            float tempDelta = deltaTime * (_bodyHeat + (1 - _bodyInsulation) * _config.bodyTemperatureChangeRate * (GetExternalTemperature() - BodyTemperature));
            BodyTemperature += tempDelta;
        }

        private float GetExternalTemperature()
        {
            return _useTestExternalTemp ? _TESTExternalTemperature : _temperatureService.GetTemperatureAtWorldPos(transform.position);
        }
    }
}

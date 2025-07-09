using Blizzard.Grid;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Blizzard.Temperature;
using TMPro;
using Zenject;


namespace Blizzard.Obstacles
{
    public class ObstacleTest : MonoBehaviour
    {
        [SerializeField] ObstacleData obstacleToPlace; // Place this obstacle to test placement
        [SerializeField] Vector2Int obstaclePosition;
        [SerializeField] ComputeShader _heatDiffusionShader;
        [SerializeField] Image _heatmap;
        [SerializeField] GameObject _player;
        [SerializeField] TextMeshProUGUI _playerTemp;
        [Header("Testing")]
        [SerializeField] bool _doTemperatureHeatStep = true;


        [Inject] private TemperatureService _temperatureService;
        [Inject] private ObstacleGridService _obstacleGridService;

        private void Update()
        {
            if (_doTemperatureHeatStep)
            {
                _temperatureService.DoHeatDiffusionStep(Time.deltaTime);
                _temperatureService.ComputeHeatmap();
            }
            UpdateHeatmap();
        }


        [Button]
        public void LogIsCellOccupied()
        {
            Debug.Log($"{obstaclePosition} occupied?: {_obstacleGridService.IsOccupied(obstaclePosition)}");
        }

        [Button]
        public void LogCellTemperature()
        {
            if (_obstacleGridService.TryGetObstacleAt(obstaclePosition, out Obstacle obstacle))
            {
                Debug.Log($"Obstacle temperature: {_temperatureService.Grid.GetAt(obstaclePosition)}");
            }
        }

        [Button]
        public void PlaceObstacle()
        {
            _obstacleGridService.PlaceObstacleAt(obstaclePosition, obstacleToPlace);
        }

        [Button]
        public void RemoveObstacle()
        {
            Debug.Log("remove success: " + _obstacleGridService.TryRemoveObstacleAt(obstaclePosition));
        }

        private void UpdateHeatmap()
        {
            _heatmap.material.SetTexture("_MainTex", _temperatureService.HeatmapTexture);
        }
    }
}

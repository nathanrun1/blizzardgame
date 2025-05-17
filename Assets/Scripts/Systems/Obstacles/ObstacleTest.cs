using Blizzard.Grid;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Blizzard.Temperature;
using TMPro;


namespace Blizzard.Obstacles
{
    public class ObstacleTest : MonoBehaviour
    {
        [SerializeField] ObstacleData obstacleToPlace; // Place this obstacle to test placement
        [SerializeField] Vector2Int obstaclePosition;
        [SerializeField] ComputeShader _heatDiffusionShader;
        [SerializeField] Image _heatmap;
        [SerializeField] float _cellSideLength = 0.25f;
        [SerializeField] GameObject _player;
        [SerializeField] TextMeshProUGUI _playerTemp;

        private TemperatureService _temperatureService;
        private ObstacleGridService _obstacleGridService;  


        private void Start()
        {
            SetupTemperatureService();
            SetupObstacleGridService();
        }

        private void Update()
        {
            _temperatureService.DoHeatDiffusionStep(Time.deltaTime);
            _temperatureService.ComputeHeatmap();
            UpdateHeatmap();
            ShowPlayerCellTemp();
        }

        private void ShowPlayerCellTemp()
        {
            Vector2 playerPos = new(_player.transform.position.x, _player.transform.position.y);
            Vector2Int gridPos = _temperatureService.Grid.WorldToCellPos(playerPos);
            _playerTemp.text = $"{_temperatureService.Grid.GetAt(gridPos).temperature}°";
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
                Debug.Log($"Obstacle temperature: {obstacle.Temperature}");
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

        private void SetupTemperatureService()
        {
            var mainGrid = new DenseWorldGrid<TemperatureCell>(_cellSideLength, _cellSideLength, 16, 16); // Arbitrary main grid dimensions
            mainGrid.Initialize(new TemperatureCell
            {
                temperature = 10, // Set initial temperature of all cells to 10
                insulation = 0,
                heat = 0
            });
            _temperatureService = new TemperatureService(
                    mainGrid,
                    new BasicDenseGrid<TemperatureCell>(16, 16),
                    _heatDiffusionShader
            );
            Debug.Log(_temperatureService);
        }

        private void UpdateHeatmap()
        {
            _heatmap.material.SetTexture("_MainTex", _temperatureService.HeatmapTexture);
        }

        private void SetupObstacleGridService()
        {
            _obstacleGridService = new ObstacleGridService(new HashWorldGrid<Obstacle>(_cellSideLength, _cellSideLength), _temperatureService); // TEMP: Hardcodeed cell dimensions
        }
    }
}

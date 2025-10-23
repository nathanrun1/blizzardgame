using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Blizzard.Temperature;
using Blizzard.Utilities.Logging;
using TMPro;
using Zenject;


namespace Blizzard.Obstacles
{
    public class ObstacleTest : MonoBehaviour
    {
        [SerializeField] private ObstacleData obstacleToPlace; // Place this obstacle to test placement
        [SerializeField] private Vector2Int obstaclePosition;
        [SerializeField] private ComputeShader _heatDiffusionShader;
        [SerializeField] private Image _heatmap;
        [SerializeField] private GameObject _player;
        [SerializeField] private TextMeshProUGUI _playerTemp;
        [Header("Testing")] [SerializeField] private bool _doTemperatureHeatStep = true;


        [Inject] private TemperatureService _temperatureService;
        [Inject] private ObstacleGridService _obstacleGridService;

        private void Update()
        {
            if (_doTemperatureHeatStep)
            {
                //_temperatureService.DoHeatDiffusionStep(Time.deltaTime);
                //_temperatureService.ComputeHeatmap();
            }

            UpdateHeatmap();
        }


        [Button]
        public void LogIsCellOccupied()
        {
            BLog.Log($"{obstaclePosition} occupied?: {_obstacleGridService.IsOccupied(obstaclePosition)}");
        }

        [Button]
        public void LogCellTemperature()
        {
            if (_obstacleGridService.TryGetObstacleAt(obstaclePosition, out var obstacle))
                BLog.Log($"Obstacle temperature: {_temperatureService.Grid.GetAt(obstaclePosition).temperature}");
        }

        [Button]
        public void PlaceObstacle()
        {
            _obstacleGridService.PlaceObstacleAt(obstaclePosition, obstacleToPlace);
        }

        [Button]
        public void RemoveObstacle()
        {
            BLog.Log("remove success: " + _obstacleGridService.TryRemoveObstacleAt(obstaclePosition));
        }

        private void UpdateHeatmap()
        {
            _heatmap.material.SetTexture("_MainTex", _temperatureService.HeatmapTexture);
        }
    }
}
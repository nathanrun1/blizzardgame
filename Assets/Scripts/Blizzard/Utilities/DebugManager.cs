using System;
using System.Diagnostics;
using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using Blizzard.Temperature;
using System.Linq;
using Blizzard.Inventory;
using Blizzard.Obstacles;
using Blizzard.UI.Core;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.Logging;

namespace Blizzard.Utilities
{
    public class DebugManager : MonoBehaviour
    {
        [Serializable]
        private struct ObstaclePlacement
        {
            public ObstacleData obstacle;
            public Vector2Int position;
        }

        [Inject] private InventoryService _inventoryService;
        [Inject] private UIService _uiService;
        [Inject] private TemperatureService _temperatureService;
        [Inject] private ObstacleGridService _obstacleGridService;

        [FoldoutGroup("UI")] [SerializeField] private int[] _startupUI;

        [FoldoutGroup("UI")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void InitUI(int id)
        {
            _uiService.InitUI(id);
        }

        [FoldoutGroup("UI")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void CloseUI(int id)
        {
            _uiService.CloseUI(id);
        }

        [FoldoutGroup("Inventory")] [SerializeField]
        private ItemAmountPair[] _startingItems;

        [FoldoutGroup("Inventory")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void InventoryAddItem(ItemData item, int amount, bool fill = true)
        {
            var added = _inventoryService.TryAddItem(item, amount, fill);
            BLog.Log($"Successfully added {added}x {item.displayName}");
        }

        [FoldoutGroup("Inventory")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void InventoryRemoveItem(ItemData item, int amount, bool drain = false)
        {
            BLog.Log(item == null);
            var removed = _inventoryService.TryRemoveItem(item, amount, drain);
            BLog.Log($"Successfully removed {removed}x {item.displayName}");
        }

        [FoldoutGroup("Inventory")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void PrintInventoryContents()
        {
            var str = "--INVENTORY--\n";
            foreach (var slot in _inventoryService.inventorySlots)
                if (slot.Empty()) str += "[EMPTY]\n";
                else str += $"[{slot.Amount}x {slot.Item.displayName}]\n";

            BLog.Log(str);
        }

        [FoldoutGroup("Obstacles")] [SerializeField]
        private ObstaclePlacement[] _initialObstacles;

        [FoldoutGroup("Obstacles")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void PlaceObstacle(ObstaclePlacement placement)
        {
            _obstacleGridService.PlaceObstacleAt(placement.position, placement.obstacle);
        }

        [FoldoutGroup("Obstacles")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void RemoveObstacleAt(Vector2Int position)
        {
            _obstacleGridService.TryRemoveObstacleAt(position);
        }

        [FoldoutGroup("Obstacles")]
        [Button]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void PrintLocationsOfObstaclesWithFlag(ObstacleFlags flags)
        {
            var str = "OBSTACLE POSITIONS:\n";
            foreach (var o in _obstacleGridService.GetAllObstaclesWithFlags(flags))
                str += o.name + " at " + o.transform.position + '\n';

            BLog.Log(str);
        }


        [FoldoutGroup("Temperature")] [SerializeField]
        private bool _temperatureOverride = false;

        [FoldoutGroup("Temperature")] [SerializeField]
        private float _diffusionFactor = 0.3f;

        [FoldoutGroup("Temperature")] [SerializeField]
        private float _ambientFactor = 0.0005f;

        [FoldoutGroup("Temperature")] [SerializeField]
        private float _ambientTemperature;

        private Camera _camera;

        [FoldoutGroup("Input")]
        [LabelText("TAB to print collider under pointer")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void PrintColliderUnderPointer()
        {
            BLog.Log($"Collider under pointer: {InputAssistant.GetColliderUnderPointer(_camera)}");
        }

        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void Start()
        {
            _camera = Camera.main;
            foreach (var uiId in _startupUI) InitUI(uiId);

            _inventoryService.TryAddItems(_startingItems.ToList());

            foreach (var placement in _initialObstacles)
                _obstacleGridService.PlaceObstacleAt(placement.position, placement.obstacle);
        }

        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        private void Update()
        {
            if (_temperatureOverride)
            {
                _temperatureService.SetComputeFloat("diffusionFactor", _diffusionFactor);
                _temperatureService.SetComputeFloat("ambientFactor", _ambientFactor);
                _temperatureService.AmbientTemperature = _ambientTemperature;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
                // TAB to print collider under pointer
                PrintColliderUnderPointer();
        }
    }
}
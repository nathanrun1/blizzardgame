using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Blizzard.Obstacles;
using Blizzard.Grid;
using Blizzard.Building;
using Blizzard.Inventory;
using ModestTree;


namespace Blizzard.UI
{   
    public class BuildUI : UIBase
    {
        public struct Args
        {
            /// <summary>
            /// Building to build, associated with item
            /// </summary>
            public BuildingData buildingData;
            /// <summary>
            /// Slot where building item is located in inventorySlots.
            /// </summary>
            public int itemSlot;
        }


        [Inject] private ObstacleGridService _obstacleGridService;
        [Inject] private InventoryService _inventoryService;
        [Inject] private InputService _inputService;

        /// <summary>
        /// How often build preview gets updated (ideally less than framerate for performance)
        /// </summary>
        [Header("Config")]
        [SerializeField] private float _updateDelay = 0.2f;
        /// <summary>
        /// Building preview is tinted with this color when location is occupied
        /// </summary>
        [SerializeField] private Color _occupiedLocationColor;

        private float _updateDelayTimer = 0f;

        private BuildingData _buildingData;
        private int _buildItemSlotIndex;
        /// <summary>
        /// Visual-only copy of the building, to preview to the player as to where the building will be placed
        /// </summary>
        private GameObject _buildingPreview;
        private GameObject _occupiedBuildingPreview;

        public override void Setup(object args)
        {
            Args buildArgs;
            try
            {
                buildArgs = (Args)args;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("Incorrect argument type given!");
            }

            _buildingData = buildArgs.buildingData;
            _buildItemSlotIndex = buildArgs.itemSlot;
            if (_buildingData == null)
            {
                throw new ArgumentException("Building Data is null!");
            }

            // Bind input
            _inputService.inputActions.Player.Build.performed += OnInputBuild;

            // Init building preview
            _buildingPreview = _buildingData.obstacleData.obstaclePrefab.CreatePreview();

            // Init occupied building preview (swapped with building preview when location occupied)
            _occupiedBuildingPreview = Instantiate(_buildingPreview);
            foreach (SpriteRenderer spriteRenderer in _occupiedBuildingPreview.GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.color *= _occupiedLocationColor;
            }
        }

        public override void Close(bool destroy = true)
        {
            // Unbind input
            _inputService.inputActions.Player.Build.performed -= OnInputBuild;

            // Destroy previews
            Destroy(_buildingPreview.gameObject);
            Destroy(_occupiedBuildingPreview.gameObject);
            
            base.Close(destroy);
        }

        private void OnInputBuild(InputAction.CallbackContext _)
        {
            if (InputAssistant.IsPointerOverUIElement()) return;

            Vector2Int mouseGridPosition = _obstacleGridService.Grids[0].WorldToCellPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (_obstacleGridService.IsOccupied(mouseGridPosition, _buildingData.obstacleData.obstacleLayer)) return; // Location occupied

            // Sanity check: Ensure item corresponding to building is the correct item
            BuildingItemData buildItem = _inventoryService.inventorySlots[_buildItemSlotIndex].item as BuildingItemData;
            Assert.That(buildItem != null && buildItem.buildingData == _buildingData, "Given inventory slot does not contain matching item to building!");

            // Remove one of the building from inventory, ensure item removed successfully
            if (_inventoryService.TryRemoveItemAt(_buildItemSlotIndex, 1) != 1)
            {
                Debug.LogError("Build item not successfully removed from inventory! Cancelling build.");
                return;
            }

            Debug.Log($"Placing at {mouseGridPosition}");
            _obstacleGridService.PlaceObstacleAt(mouseGridPosition, _buildingData.obstacleData);
        }

        private void Update()
        {
            _updateDelayTimer += Time.unscaledDeltaTime;
            if (_updateDelayTimer > _updateDelay)
            {
                _updateDelayTimer -= _updateDelay;
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            Debug.Log("updating preview");
            Vector2Int mouseGridPosition = _obstacleGridService.Grids[0].WorldToCellPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            GameObject preview;
            if (_obstacleGridService.IsOccupied(mouseGridPosition, _buildingData.obstacleData.obstacleLayer))
            {
                preview = _occupiedBuildingPreview;
                _occupiedBuildingPreview.SetActive(true);
                _buildingPreview.SetActive(false);
            } 
            else
            {
                preview = _buildingPreview;
                _occupiedBuildingPreview.SetActive(false);
                _buildingPreview.SetActive(true);
            }

            preview.transform.position = _obstacleGridService.Grids[0].CellToWorldPosCenter(mouseGridPosition);
        }
    }
}

using System;
using Blizzard.Constants;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;
using Blizzard.Inventory.Itemtypes;
using Blizzard.Player.Tools;
using Blizzard.UI;
using Blizzard.UI.Core;
using Blizzard.Utilities.Logging;
using Blizzard.Utilities.DataTypes;
using Object = UnityEngine.Object;

namespace Blizzard.Player
{
    /// <summary>
    /// Manages player data and interactions
    /// </summary>
    public class PlayerService : IInitializable
    {
        [Inject] private DiContainer _diContainer;
        [Inject] private UIService _uiService;
        
        /// <summary>
        /// Currently equipped tool instance
        /// </summary>
        public ToolBehaviour EquippedTool { get; private set; }
        /// <summary>
        /// Player's current position
        /// </summary>
        public Vector2 PlayerPosition => _playerCtrl.transform.position;
        /// <summary>
        /// Player object's transform
        /// </summary>
        public Transform PlayerTransform => _playerCtrl.transform;
        /// <summary>
        /// Player's current health
        /// </summary>
        public int PlayerHealth { get; private set; }
        
        /// <summary>
        /// To invoke on initializations
        /// </summary>
        private Action _initialize;

        private PlayerCtrl _playerCtrl;
        private PlayerMovement _playerMovement;

        public PlayerService(PlayerCtrl playerPrefab, Transform environment, CinemachineCamera cinemachineCamera)
        {
            _initialize = () => InitPlayer(playerPrefab, environment, cinemachineCamera);
        }

        public void Initialize()
        {
            _initialize.Invoke();
            _initialize = null;
        }

        /// <summary>
        /// Initializes player object in the scene
        /// </summary>
        private void InitPlayer(PlayerCtrl playerPrefab, Transform environment, CinemachineCamera cinemachineCamera)
        {
            // Get references to player components
            _playerCtrl =
                _diContainer.InstantiatePrefabForComponent<PlayerCtrl>(playerPrefab,
                    environment); // Initialize player obj
            _playerMovement = _playerCtrl.GetComponent<PlayerMovement>();

            // Set camera tracking target to player
            cinemachineCamera.Target.TrackingTarget = _playerCtrl.transform; 
            
            // Set initial player health
            PlayerHealth = PlayerConstants.PlayerStartHealth;
        }
        
        public void DamagePlayer(int damage, DamageFlags damageFlags)
        {
            // TODO: implement
            BLog.Log("Inflicted " + damage + " damage to player!");
            PlayerHealth -= damage;
            
            // Color flash FX
            var colorFlashArgs = new ColorFlashUI.Args
            {
                color = FXConstants.DamageFlashColor[damageFlags],
                duration = 0.2f
            };
            _uiService.InitUI(UIID.ColorFlash, colorFlashArgs);
        }

        // -- Helpers --

        /// <summary>
        /// Equips tool given tool item data
        /// </summary>
        public void EquipTool(ToolItemData toolItemData)
        {
            UnequipTool();

            // Instantiate tool prefab, parent to player transform
            EquippedTool =
                _diContainer.InstantiatePrefabForComponent<ToolBehaviour>(toolItemData.toolPrefab,
                    _playerCtrl.toolParent.transform);
        }

        /// <summary>
        /// Unequips the currently equipped tool, if any
        /// </summary>
        public void UnequipTool()
        {
            if (EquippedTool)
            {
                Object.Destroy(EquippedTool.gameObject); // TODO: obj pooling
            }
        }

        /// <summary>
        /// Returns unit vector pointing in the direction the player is facing in
        /// </summary>
        /// <returns></returns>
        public Vector2 GetFacingDirection()
        {
            var playerAngle =
                Mathf.Deg2Rad *
                (_playerMovement.playerObj.transform.eulerAngles.z + 90); // 90 degrees is player sprite rotation offset

            return new Vector2(Mathf.Cos(playerAngle), Mathf.Sin(playerAngle));
        }
    }
}
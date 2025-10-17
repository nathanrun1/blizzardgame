using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using Zenject;
using Blizzard.Inventory;
using Blizzard.Inventory.Itemtypes;
using Blizzard.Player.Tools;

namespace Blizzard.Player
{
    /// <summary>
    /// Manages player data and interactions
    /// </summary>
    public class PlayerService : IInitializable
    {
        public PlayerCtrl PlayerCtrl;
        public PlayerMovement PlayerMovement;
        public PlayerTemperature PlayerTemperature;

        public ToolBehaviour EquippedTool { get; private set; }

        public Vector2 PlayerPosition { get => PlayerCtrl.transform.position; }

        /// <summary>
        /// To invoke on initializations
        /// </summary>
        private Action _initialize;


        [Inject] DiContainer _diContainer;

        public PlayerService(PlayerCtrl playerPrefab, Transform environment, CinemachineCamera cinemachineCamera)
        {
            _initialize = () => InitPlayer(playerPrefab, environment, cinemachineCamera);
        }

        public void Initialize()
        {
            _initialize.Invoke();
            _initialize = null;
        }

        public void DamagePlayer(int damage)
        {
            // TODO: implement
            // Debug.Log("Inflicted " + damage + " damage to player!");
        }

        /// <summary>
        /// Initializes player object in the scene
        /// </summary>
        private void InitPlayer(PlayerCtrl playerPrefab, Transform environment, CinemachineCamera cinemachineCamera)
        {
            // Debug.Log(playerPrefab);

            PlayerCtrl = _diContainer.InstantiatePrefabForComponent<PlayerCtrl>(playerPrefab, environment); // Initialize player obj
            PlayerMovement = PlayerCtrl.GetComponent<PlayerMovement>();
            PlayerTemperature = PlayerCtrl.GetComponent<PlayerTemperature>();

            cinemachineCamera.Target.TrackingTarget = this.PlayerCtrl.transform; // Set camera tracking target to player
        }


        // -- Helpers --

        /// <summary>
        /// Equips tool given tool item data
        /// </summary>
        public void EquipTool(ToolItemData toolItemData)
        {
            UnequipTool();

            // Instantiate tool prefab, parent to player transform
            EquippedTool = _diContainer.InstantiatePrefabForComponent<ToolBehaviour>(toolItemData.toolPrefab, this.PlayerCtrl.toolParent.transform);
        }

        /// <summary>
        /// Unequips the currently equipped tool, if any
        /// </summary>
        public void UnequipTool()
        {
            Debug.Log("Unequipping currently equipped tool: " + EquippedTool);
            if (EquippedTool)
            {
                Debug.Log("Tool exists");
                MonoBehaviour.Destroy(EquippedTool.gameObject); // TODO: obj pooling
            }
        }

        /// <summary>
        /// Returns unit vector pointing in the direction the player is facing in
        /// </summary>
        /// <returns></returns>
        public Vector2 GetFacingDirection()
        {
            float playerAngle = Mathf.Deg2Rad * (PlayerMovement.playerObj.transform.eulerAngles.z + 90); // 90 degrees is player sprite rotation offset
            Debug.Log("Player angle: " + playerAngle);

            return new Vector2(Mathf.Cos(playerAngle), Mathf.Sin(playerAngle));
        }
    }
}

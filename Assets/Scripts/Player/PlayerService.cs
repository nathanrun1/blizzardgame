using System;
using Blizzard.Inventory;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Blizzard.Player
{
    /// <summary>
    /// Manages player data and interactions
    /// </summary>
    public class PlayerService : IInitializable
    {
        public PlayerCtrl playerCtrl;

        public ToolBehaviour equippedTool { get; private set; }

        /// <summary>
        /// To invoke on initializationc
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

        /// <summary>
        /// Initializes player object in the scene
        /// </summary>
        private void InitPlayer(PlayerCtrl playerPrefab, Transform environment, CinemachineCamera cinemachineCamera)
        {
            Debug.Log(playerPrefab);
            this.playerCtrl = _diContainer.InstantiatePrefabForComponent<PlayerCtrl>(playerPrefab, environment); // Initialize player obj
            cinemachineCamera.Target.TrackingTarget = this.playerCtrl.transform; // Set camera tracking target to player
        }


        // -- Helpers --

        /// <summary>
        /// Equips tool given tool item data
        /// </summary>
        public void EquipTool(ToolItemData toolItemData)
        {
            UnequipTool();

            // Instantiate tool prefab, parent to player transform
            equippedTool = _diContainer.InstantiatePrefabForComponent<ToolBehaviour>(toolItemData.toolPrefab, this.playerCtrl.toolParent.transform);
        }

        /// <summary>
        /// Unequips the currently equipped tool, if any
        /// </summary>
        public void UnequipTool()
        {
            if (equippedTool != null)
            {
                MonoBehaviour.Destroy(equippedTool); // TODO: obj pooling
            }
        }
    }
}

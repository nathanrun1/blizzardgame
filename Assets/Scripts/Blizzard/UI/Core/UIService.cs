using System;
using System.Collections.Generic;
using Blizzard.Obstacles;
using UnityEngine;
using Zenject;
using Blizzard.Utilities.Logging;


namespace Blizzard.UI.Core
{
    public class UIService
    {
        /// <summary>
        /// Parent of instantiated UI prefabs
        /// </summary>
        private readonly RectTransform _uiParent;

        [Inject] private DiContainer _diContainer;

        private readonly Dictionary<int, UIData> _intDict = new();
        private readonly Dictionary<string, UIData> _strDict = new();

        /// <summary>
        /// Active UI instances, mapped by ID
        /// </summary>
        private readonly Dictionary<int, UIBase> _activeUI = new();

        /// <summary>
        /// Inactive but ready UI instances, pooled for reuse, mapped by ID
        /// </summary>
        private readonly Dictionary<int, UIBase> _inactiveUI = new();


        /// <summary>
        /// The transform of a GameObject that is always at the foreground of the UI Canvas. 
        /// Useful as a parent to UI elements that should be rendered on top of everything.
        /// </summary>
        public Transform CanvasTop
        {
            get
            {
                // Ensure always at foreground when retrieved
                _canvasTop.SetAsLastSibling();
                return _canvasTop;
            }
        }
        private readonly Transform _canvasTop;


        public UIService(RectTransform uiParent)
        {
            BLog.Log("DI Container: " + _diContainer);
            _uiParent = uiParent;

            // Initialize CanvasTop
            _canvasTop = new GameObject("CanvasTop").transform;
            _canvasTop.SetParent(_uiParent);
            _canvasTop.SetAsLastSibling();
        }
        
        [Inject]
        private void InitDictionaries(UIDatabase uiDatabase)
        {
            BLog.Log("Initializing UI prefab dictionaries");
            foreach (var uiData in uiDatabase.uiDatas)
            {
                _intDict.Add(uiData.id, uiData);
                _strDict.Add(uiData.stringId, uiData);
            }
        }

        /// <summary>
        /// Initializes UI prefab of given id.
        /// First instantiates it, then parents it to the appropriate canvas, then calls the Setup() method with provided args.
        /// </summary>
        /// <returns>Reference to instantiated instance of the UI prefab</returns>
        public UIBase InitUI(UIID id, object args = null)
        {
            UIData uiData;
            if (_intDict.TryGetValue((int)id, out var value)) uiData = value;
            else
                throw new KeyNotFoundException("No UI prefab exists with this id: " + id);

            return InitUI(uiData, args);
        }

        /// <summary>
        /// Closes UI of given id, if it is a singleton UI and there is an active instance of it
        /// </summary>
        public void CloseUI(UIID id)
        {
            if (!_activeUI.TryGetValue((int)id, out var uiObj)) return;

            uiObj.Close();
        }

        /// <summary>
        /// If no instance of the UI of given id is active, initializes an instance of it with given args.
        /// If an instance is active, closes it.
        /// </summary>
        public void ToggleUI(UIID id, object args = null)
        {
            if (IsUIActive(id)) CloseUI(id);
            else InitUI(id, args);
        }

        /// <summary>
        /// Fetches a reference to a singleton UI instance. Returns it if it exists.
        /// </summary>
        public UIBase GetSingletonUI(UIID id)
        {
            if (!_activeUI.TryGetValue((int)id, out var uiObj))
            {
                BLog.LogError(
                    $"Attempted to get singleton instance of UI (id {id}), but not open or isSingle set to false");
                return null;
            }

            return uiObj;
        }

        /// <summary>
        /// Determines if there exists an active UI instance of the given id.
        /// </summary>
        public bool IsUIActive(UIID id)
        {
            return _activeUI.ContainsKey((int)id);
        }

        private UIBase InitUI(UIData uiData, object args)
        {
            if (uiData.isSingle && _activeUI.TryGetValue(uiData.id, out UIBase uiObj))
            {
                // Singleton and already active, just setup with new data
                uiObj.Setup(args);
                return uiObj;
            } 
            
            if (!_inactiveUI.TryGetValue(uiData.id, out uiObj)) // Prioritize inactive pool before instantiating
            {
                // None in inactive pool, instantiate new instance
                uiObj = _diContainer.InstantiatePrefabForComponent<UIBase>(uiData.uiPrefab);
                uiObj.SetParent(_uiParent);
                uiObj.Setup(args);

                uiObj.OnClose += (destroyed) =>
                {
                    _activeUI.Remove(uiData.id);
                    if (!destroyed) _inactiveUI.Add(uiData.id, uiObj);
                };
                
                // Don't destroy on close only if: not singleton and destroyOnClose is false
                uiObj.SetMetadata(new UIMetadata(
                    destroyOnClose: !uiData.isSingle || uiData.destroyOnClose
                    ));
            }
            else
            {
                uiObj.Setup(args);  // Setup before active
                uiObj.gameObject.SetActive(true);
                _inactiveUI.Remove(uiData.id); // No longer inactive
            }

            if (uiData.isSingle) _activeUI.Add(uiData.id, uiObj); // Track active UI instance only if single

            return uiObj;
        }

        /// <summary>
        /// Fetches a reference to a singleton UI instance. Returns it if it exists.
        /// </summary>
        [Obsolete]
        public UIBase GetSingletonUI(string stringId)
        {
            var id = _strDict[stringId].id;
            return GetSingletonUI((UIID)id);
        }
        
        /// <summary>
        /// Closes UI of given string id, if it is a singleton UI and there is an active instance of it
        /// </summary>
        [Obsolete]
        public void CloseUI(string stringId)
        {
            var id = _strDict[stringId].id;
            CloseUI((UIID)id);
        }
        
        /// <summary>
        /// Initializes UI prefab of given string id.
        /// First instantiates it, then parents it to the appropriate canvas, then calls the Setup() method with provided args.
        /// </summary>
        /// <returns>Reference to instantiated instance of the UI prefab</returns>
        [Obsolete]
        public UIBase InitUI(string stringId, object args = null)
        {
            UIData uiData;
            if (_strDict.TryGetValue(stringId, out var value)) uiData = value;
            else
                throw new KeyNotFoundException("No UI prefab exists with this id: " + stringId);

            return InitUI(uiData, args);
        }
    }
}
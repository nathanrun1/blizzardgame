using System.Collections.Generic;
using UnityEngine;
using Zenject;


namespace Blizzard.UI
{

    public class UIService
    {
        /// <summary>
        /// Parent of instantiated UI prefabs
        /// </summary>
        private RectTransform _uiParent;

        [Inject] DiContainer _diContainer;

        private Dictionary<int, UIData> _intDict = new Dictionary<int, UIData>();
        private Dictionary<string, UIData> _strDict = new Dictionary<string, UIData>();

        /// <summary>
        /// Active UI instances, mapped by ID
        /// </summary>
        private Dictionary<int, UIBase> _activeUI = new Dictionary<int, UIBase>();

        /// <summary>
        /// Inactive but ready UI instances, pooled for reuse, mapped by ID
        /// </summary>
        private Dictionary<int, UIBase> _inactiveUI = new Dictionary<int, UIBase>();

        public UIService(UIDatabase uiDatabase, RectTransform uiParent)
        {
            Debug.Log("DI Container: " + _diContainer);
            this._uiParent = uiParent;

            InitDictionaries(uiDatabase);
        }

        /// <summary>
        /// Initializes UI prefab of given id.
        /// First instantiates it, then parents it to the appropriate canvas, then calls the Setup() method with provided args.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public void InitUI(int id, object args = null)
        {
            UIData uiData;
            if (_intDict.ContainsKey(id)) uiData = _intDict[id];
            else
            {
                throw new KeyNotFoundException("No UI prefab exists with this id: " + id);
            }

            InitUI(uiData, args);
        }

        /// <summary>
        /// Initializes UI prefab of given string id.
        /// First instantiates it, then parents it to the appropriate canvas, then calls the Setup() method with provided args.
        /// </summary>
        public void InitUI(string stringId, object args = null)
        {
            UIData uiData;
            if (_strDict.ContainsKey(stringId)) uiData = _strDict[stringId];
            else
            {
                throw new KeyNotFoundException("No UI prefab exists with this id: " + stringId);
            }

            InitUI(uiData, args);
        }

        /// <summary>
        /// Closes UI of given id, if there is an active instance of it
        /// </summary>
        public void CloseUI(int id)
        {
            if (!_activeUI.ContainsKey(id))
            {
                Debug.LogError($"Attempted to close UI (id {id}), but not open or isSingle set to false");
                return;
            }

            bool destroyOnClose = _intDict[id].destroyOnClose;
            UIBase uiObj = _activeUI[id];
            uiObj.Close(destroyOnClose);
        }

        /// <summary>
        /// Closes UI of given string id, if there is an active instance of it
        /// </summary>
        public void CloseUI(string stringId)
        {
            int id = _strDict[stringId].id;
            CloseUI(id);
        }


        private void InitUI(UIData uiData, object args) 
        {
            UIBase uiObj;
            if (!_inactiveUI.TryGetValue(uiData.id, out uiObj)) // Prioritize inactive pool before instantiating
            {
                uiObj = _diContainer.InstantiatePrefabForComponent<UIBase>(uiData.uiPrefab);
                uiObj.SetParent(_uiParent);

                if (uiData.isSingle && !uiData.destroyOnClose)
                {
                    // Set instance as inactive rather than destroy
                    uiObj.OnClose += () =>
                    {
                        _activeUI.Remove(uiData.id);
                        _inactiveUI.Add(uiData.id, uiObj);
                    };
                }
                else if (uiData.isSingle)
                {
                    // Remove instance from active list before destroying
                    uiObj.OnClose += () =>
                    {
                        _activeUI.Remove(uiData.id);
                    };
                }
            }
            else
            {
                uiObj.gameObject.SetActive(true);
                _inactiveUI.Remove(uiData.id); // No longer inactive
            }

            uiObj.Setup(args);

            if (uiData.isSingle)
            {
                _activeUI.Add(uiData.id, uiObj); // Track active UI instance only if single
            }
        }


        private void InitDictionaries(UIDatabase uiDatabase)
        {
            Debug.Log("Initializing UI prefab dictionaries");
            foreach (UIData uiData in uiDatabase.uiDatas)
            {
                _intDict.Add(uiData.id, uiData);
                _strDict.Add(uiData.stringId, uiData);
            }
        }
    }
}

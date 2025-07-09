using System.Collections.Generic;
using UnityEngine;
using Zenject;


namespace Blizzard.UI
{

    public class UIService
    {
        /// <summary>
        /// Database of instantiatable UI prefabs
        /// </summary>
        private UIDatabase _uiDatabase;
        /// <summary>
        /// Parent of instantiated UI prefabs
        /// </summary>
        private RectTransform _uiParent;

        [Inject] DiContainer _diContainer;

        private Dictionary<int, UIBase> _intPrefabDict = new Dictionary<int, UIBase>();
        private Dictionary<string, UIBase> _strPrefabDict = new Dictionary<string, UIBase>();

        public UIService(UIDatabase uiDatabase, RectTransform uiParent)
        {
            this._uiDatabase = uiDatabase;
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
            UIBase uiPrefab;
            if (_intPrefabDict.ContainsKey(id)) uiPrefab = _intPrefabDict[id];
            else
            {
                throw new KeyNotFoundException("No UI prefab exists with this id: " + id);
            }

            UIBase uiObj = _diContainer.InstantiatePrefabForComponent<UIBase>(uiPrefab);

            uiObj.SetParent(_uiParent);
            uiObj.Setup(args);
        }

        /// <summary>
        /// Initializes UI prefab of given string id.
        /// First instantiates it, then parents it to the appropriate canvas, then calls the Setup() method with provided args.
        /// </summary>
        public void InitUI(string stringId, object args = null)
        {
            UIBase uiPrefab;
            if (_strPrefabDict.ContainsKey(stringId)) uiPrefab = _strPrefabDict[stringId];
            else
            {
                throw new KeyNotFoundException("No UI prefab exists with this id: " + stringId);
            }

            UIBase uiObj = _diContainer.InstantiatePrefabForComponent<UIBase>(uiPrefab);

            uiObj.SetParent(_uiParent);
            uiObj.Setup(args);
        }


        private void InitDictionaries(UIDatabase uiDatabase)
        {
            Debug.Log("Initializing UI prefab dictionaries");
            foreach (UIData uiData in uiDatabase.uiDatas)
            {
                _intPrefabDict.Add(uiData.id, uiData.uiPrefab);
                _strPrefabDict.Add(uiData.stringId, uiData.uiPrefab);
            }
        }
    }
}

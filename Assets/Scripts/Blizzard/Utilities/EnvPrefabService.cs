using System.Collections.Generic;
using Zenject;
using UnityEngine;

namespace Blizzard.Utilities
{
    /// <summary>
    /// Manages environmental prefab references and instantiation
    /// </summary>
    public class EnvPrefabService
    {
        public Transform environmentParent;

        private Dictionary<int, GameObject> _intIdDict = new();
        private Dictionary<string, GameObject> _stringIdDict = new();

        [Inject] private DiContainer _diContainer;

        public EnvPrefabService(EnvironmentDatabase database, Transform environmentParent)
        {
            this.environmentParent = environmentParent;
            InitDictionaries(database);
        }

        public GameObject GetPrefab(int id)
        {
            try
            {
                return _intIdDict[id];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("No environment prefab exists with id: " + id);
            }
        }

        public GameObject GetPrefab(string stringId)
        {
            try
            {
                return _stringIdDict[stringId];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("No environment prefab exists with string id: " + stringId);
            }
        }

        /// <summary>
        /// Instantiates environment prefab and parents it to environment
        /// </summary>
        public GameObject InstantiatePrefab(int id)
        {
            var prefab = GetPrefab(id);
            return _diContainer.InstantiatePrefab(prefab, environmentParent);
        }

        /// <summary>
        /// Instantiates environment prefab and parents it to environment
        /// </summary>
        public GameObject InstantiatePrefab(string stringId)
        {
            var prefab = GetPrefab(stringId);
            return _diContainer.InstantiatePrefab(prefab, environmentParent);
        }


        private void InitDictionaries(EnvironmentDatabase database)
        {
            foreach (var envPrefabData in database.environmentPrefabs)
            {
                _intIdDict[envPrefabData.id] = envPrefabData.prefab;
                _stringIdDict[envPrefabData.stringId] = envPrefabData.prefab;
            }
        }
    }
}
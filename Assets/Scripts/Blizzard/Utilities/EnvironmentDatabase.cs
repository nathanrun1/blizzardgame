using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.Utilities
{
    [Serializable]
    public struct EnvPrefabData
    {
        public int id;
        public string stringId;
        public GameObject prefab;
    }

    /// <summary>
    /// Database of environment prefabs
    /// </summary>
    [CreateAssetMenu(fileName = "EnvironmentDatabase", menuName = "ScriptableObjects/Utilities/EnvironmentDatabase")]
    public class EnvironmentDatabase : ScriptableObject
    {
        public EnvPrefabData[] environmentPrefabs;
    }
}
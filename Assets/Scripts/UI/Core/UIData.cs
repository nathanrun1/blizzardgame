using System;
using UnityEngine;

namespace Blizzard.UI
{
    /// <summary>
    /// A UI prefab that can be instantiated through UIService
    /// </summary>
    [CreateAssetMenu(fileName = "UIData", menuName = "ScriptableObjects/UI/UIData")]
    public class UIData : ScriptableObject
    {
        /// <summary>
        /// Unique id
        /// </summary>
        public int id;
        /// <summary>
        /// Unique string id (for readability)
        /// </summary>
        public string stringId;
        /// <summary>
        /// Prefab of UI element, parented to Main Canvas on instantiation
        /// </summary>
        public UIBase uiPrefab;
    }
}

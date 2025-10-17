using System;
using UnityEngine;

namespace Blizzard.UI.Core
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
        /// <summary>
        /// Whether only one instance of this ui can be active
        /// </summary>
        public bool isSingle;
        /// <summary>
        /// Whether to destroy the ui prefab when it is closed, and thus to reinstantiate it every time it is opened.
        /// Ignored if isSingle set to false (assumed to be true)
        /// </summary>
        public bool destroyOnClose;
    }
}

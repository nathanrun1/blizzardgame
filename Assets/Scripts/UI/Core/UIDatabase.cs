using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.UI
{
    /// <summary>
    /// A UI prefab that can be instantiated through UIService
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "ScriptableObjects/UI/UIDatabase")]
    public class UIDatabase : ScriptableObject
    {
        public UIData[] uiDatas;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.UI
{
    /// <summary>
    /// Database of UI prefabs instantiatable through UIService
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "ScriptableObjects/UI/UIDatabase")]
    public class UIDatabase : ScriptableObject
    {
        public UIData[] uiDatas;
    }
}

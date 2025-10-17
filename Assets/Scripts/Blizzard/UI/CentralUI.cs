using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Blizzard.UI
{
    public interface IUIState
    {
        /// <summary>
        /// Enter this UI State
        /// </summary>
        public void Enter();
        /// <summary>
        /// Call state's update function, to be invoked once per frame
        /// </summary>
        public void Update();
        /// <summary>
        /// Exit this state
        /// </summary>
        public void Exit();
    }

    /// <summary>
    /// Central UI class, manages player input in different UI modes
    /// </summary>
    public class CentralUI : MonoBehaviour
    {

    }
}
using UnityEngine;


namespace Blizzard.UI
{
    /// <summary>
    /// Base class that UI prefabs instantiatable from UIService should inherit from.
    /// Provides functionality to pass arguments to it.
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        protected RectTransform _parent;

        public void SetParent(RectTransform parent)
        {
            Debug.Log((transform as RectTransform).sizeDelta);
            transform.SetParent(parent, worldPositionStays: false);
            this._parent = parent;
        }

        /// <summary>
        /// Sets up this UI element given data
        /// </summary>
        /// <param name="data"></param>
        public abstract void Setup(object args);
    }
}

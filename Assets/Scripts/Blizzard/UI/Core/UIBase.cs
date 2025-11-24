using System;
using UnityEngine;
using Blizzard.Utilities.Logging;


namespace Blizzard.UI.Core
{
    public struct UIMetadata
    {
        /// <summary>
        /// Whether the UIBase instance destroys itself when closed, or instead sets itself inactive.
        /// </summary>
        public readonly bool destroyOnClose;

        public UIMetadata(bool destroyOnClose)
        {
            this.destroyOnClose = destroyOnClose;
        }
    }
    
    /// <summary>
    /// Base class that UI prefabs instantiatable from UIService should inherit from.
    /// Provides functionality to pass arguments to it.
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        /// <summary>
        /// Metadata about the UI
        /// </summary>
        private UIMetadata _metadata;
        
        /// <summary>
        /// Parent transform of UI
        /// </summary>
        protected RectTransform _parent;

        /// <summary>
        /// Invoked when UI instance is about to be closed.
        ///
        /// Args: (Whether UI instance will be destroyed: bool)
        /// </summary>
        public event Action<bool> OnClose;

        public void SetParent(RectTransform parent)
        {
            transform.SetParent(parent, false);
            _parent = parent;
        }

        /// <summary>
        /// Sets the metadata of this UI instance
        /// </summary>
        public void SetMetadata(UIMetadata metadata)
        {
            _metadata = metadata;
        }

        /// <summary>
        /// Sets up this UI element given ID and args.
        /// If re-activating UI, called before re-activated.
        /// </summary>
        public abstract void Setup(object args);

        /// <summary>
        /// Closes the UI
        /// </summary>
        public virtual void Close()
        {
            OnClose?.Invoke(_metadata.destroyOnClose);
            if (_metadata.destroyOnClose) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
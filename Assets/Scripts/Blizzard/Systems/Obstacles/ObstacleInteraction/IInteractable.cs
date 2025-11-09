using System;
using UnityEngine.EventSystems;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// An interactable object
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Description of the primary interaction
        /// </summary>
        public string PrimaryInteractText { get; }
        
        /// <summary>
        /// Whether the primary interaction can currently be activated
        /// </summary>
        public bool PrimaryInteractReady { get; }

        /// <summary>
        /// Invoked when the player triggers a primary interaction.
        /// </summary>
        public void OnPrimaryInteract();
    }
    
    /// <summary>
    /// An interactable object with a secondary interaction
    /// </summary>
    public interface ISecondaryInteractable : IInteractable
    {
        /// <summary>
        /// Description of the secondary interaction
        /// </summary>
        public string SecondaryInteractText { get; } 
        
        /// <summary>
        /// Whether the secondary interaction can currently be activated
        /// </summary>
        public bool SecondaryInteractReady { get; }
        
        /// <summary>
        /// Invoked when the player triggers a secondary interaction
        /// </summary>
        public void OnSecondaryInteract();
    }
}
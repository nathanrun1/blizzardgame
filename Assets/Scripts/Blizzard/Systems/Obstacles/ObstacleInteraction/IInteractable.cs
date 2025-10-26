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
        /// Invoked when the player triggers a secondary interaction
        /// </summary>
        public void OnSecondaryInteract();
    }
}
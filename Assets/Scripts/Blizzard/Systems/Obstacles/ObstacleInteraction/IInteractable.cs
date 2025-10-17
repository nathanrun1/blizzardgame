using UnityEngine.EventSystems;

namespace Blizzard.Obstacles
{
    interface IInteractable 
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
}
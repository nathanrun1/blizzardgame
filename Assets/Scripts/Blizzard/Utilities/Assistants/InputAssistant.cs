using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Blizzard.Utilities.Assistants
{
    public static class InputAssistant
    {
        /// <summary>
        /// Determines whether the pointer is currently over a UI element
        /// </summary>
        /// <returns>True if so, false otherwise</returns>
        public static bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }

        private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
        {
            return eventSystemRaycastResults.Any(result => result.gameObject.layer == LayerMask.NameToLayer("UI"));
        }

        private static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = UnityEngine.Input.mousePosition
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            return raycastResults;
        }


        public static Collider2D GetColliderUnderPointer(Camera camera)
        {
            Vector2 mouseWorld = camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            return Physics2D.OverlapPoint(mouseWorld);
        }
    }
}

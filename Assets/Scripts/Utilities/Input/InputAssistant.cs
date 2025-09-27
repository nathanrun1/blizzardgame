using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Blizzard
{
    public static class InputAssistant
    {
        /// <summary>
        /// Determines whether or not the pointer is currently over a UI element
        /// </summary>
        /// <returns>True if so, false otherwise</returns>
        public static bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }

        private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
        {
            foreach (RaycastResult result in eventSystemRaycastResults)
            {
                if (result.gameObject.layer == LayerMask.NameToLayer("UI")) return true;
            }

            return false;
        }

        private static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            return raycastResults;
        }


        public static Collider2D GetColliderUnderPointer()
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return Physics2D.OverlapPoint(mouseWorld);
        }
    }
}

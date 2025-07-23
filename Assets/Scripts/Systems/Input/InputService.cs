using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Blizzard
{
    public class InputService
    {
        public PlayerInputActions inputActions;

        public InputService()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable(); // Enabled by default
        }

        public bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }

        private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
        {
            foreach (RaycastResult result in eventSystemRaycastResults)
            {
                if (result.gameObject.layer == LayerMask.NameToLayer("UI")) return true;
            }

            return false;
        }

        private List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            return raycastResults;
        }
    }
}

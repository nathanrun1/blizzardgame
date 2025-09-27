using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;

namespace Blizzard.Obstacles.Concrete
{
    public class Furnace : Structure, IInteractable
    {
        public string PrimaryInteractText { get; private set; } = "Smelt";

        public void OnPrimaryInteract()
        {
            Debug.Log("FURNACE INTERACTION!");
        }
    }
}
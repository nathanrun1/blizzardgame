using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;
using Blizzard.Inventory;

namespace Blizzard.Obstacles.Concrete
{
    public class Furnace : Structure, IInteractable
    {
        public string PrimaryInteractText { get; private set; } = "Smelt";


        private InventorySlot _ingredient;
        private InventorySlot _fuel;
        private InventorySlot _result;

        public void OnPrimaryInteract()
        {
            
        }
    }
}
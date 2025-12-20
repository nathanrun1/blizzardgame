using System;
using Blizzard.Inventory;
using Blizzard.Obstacles.Concrete;
using Blizzard.UI.Core;
using Blizzard.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class CampfireUI : UIBase
{
    public struct Args
    {
        /// <summary>
        /// Linked campfire
        /// </summary>
        public Campfire campfire;
    }
    
    [Header("References")]
    [SerializeField] private InventorySlotCtrl _fuelSlotUI;
    [SerializeField] private InventorySlotCtrl _reformSlotUI;
    [SerializeField] private Button _rebuildButton;
    [SerializeField] private Button _starFormButton;
    [SerializeField] private Button _teepeeFormButton;
    [SerializeField] private Button _cabinFormButton;

    private Campfire _campfire;

    public override void Setup(object args)
    {
        Args campfireArgs;
        try
        {
            campfireArgs = (Args)args;
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException("Incorrect argument type given!");
        }

        _campfire = campfireArgs.campfire;
    }

    private void SetupUI()
    {
        _fuelSlotUI.LinkedSetup(_campfire.fuelSlot);
        _reformSlotUI.LinkedSetup(_campfire.reformSlot);
    }
}

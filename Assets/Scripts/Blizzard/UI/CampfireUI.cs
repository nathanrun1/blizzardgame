using System;
using Blizzard.Input;
using Blizzard.Inventory;
using Blizzard.Obstacles.Concrete;
using Blizzard.UI.CampfireUI;
using Blizzard.UI.Core;
using Blizzard.UI.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class CampfireUI : UIBase
{
    [Inject] private InputService _inputService;
    
    public struct Args
    {
        /// <summary>
        /// Linked campfire
        /// </summary>
        public Campfire campfire;
    }
    
    [Header("References")]
    [SerializeField] private InventorySlotCtrl _fuelSlotUI;
    [FormerlySerializedAs("_reformSlotUI")] [SerializeField] private InventorySlotCtrl _rebuildSlotUI;
    [SerializeField] private Button _rebuildButton;
    [SerializeField] private CampfireFormButton _starFormButton;
    [SerializeField] private CampfireFormButton _teepeeFormButton;
    [SerializeField] private CampfireFormButton _cabinFormButton;
    [SerializeField] private Transform _formSelector;

    private Campfire _campfire;
    private CampfireForm _selectedForm;

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

        _inputService.inputActions.UI.Cancel.performed += OnUICancelInput;
        
        _campfire = campfireArgs.campfire;
        SetupUI();
    }

    public override void Close()
    {
        _inputService.inputActions.UI.Cancel.performed -= OnUICancelInput;
        base.Close();
    }

    private void SetupUI()
    {
        _fuelSlotUI.LinkedSetup(_campfire.fuelSlot, true, true);
        _rebuildSlotUI.LinkedSetup(_campfire.rebuildSlot, true, true);
        _formSelector.gameObject.SetActive(false);
    }
    
    private void OnUICancelInput(InputAction.CallbackContext ctx)
    {
        Close();
    }

    private void SelectForm(CampfireForm form)
    {
        switch (form)
        {
            case CampfireForm.Star:
                MoveSelectorToButton(_starFormButton.transform);
                break;
            case CampfireForm.Teepee:
                MoveSelectorToButton(_teepeeFormButton.transform);
                break;
            case CampfireForm.Cabin:
                MoveSelectorToButton(_cabinFormButton.transform);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(form), form, null);
        }
        _selectedForm = CampfireForm.Star;
    }

    private void MoveSelectorToButton(Transform button)
    {
        _formSelector.gameObject.SetActive(true);
        _formSelector.SetParent(button);
        _formSelector.SetAsLastSibling();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(button as RectTransform); // Might need this
    }
}

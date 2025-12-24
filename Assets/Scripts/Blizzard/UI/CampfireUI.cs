using System;
using Blizzard.Input;
using Blizzard.Obstacles.Concrete;
using Blizzard.UI.Core;
using Blizzard.UI.Inventory;
using Blizzard.Utilities.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace Blizzard.UI
{
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
            BLog.Log("CampfireUI", "Setup");
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
            _selectedForm = _campfire.curForm;
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
            SetupButtons();
            RefreshActiveForm();
        }
    
        private void OnUICancelInput(InputAction.CallbackContext ctx)
        {
            Close();
        }

        private void SetupButtons()
        {
            _starFormButton.button.onClick.AddListener(() => SelectForm(CampfireForm.Star));
            _teepeeFormButton.button.onClick.AddListener(() => SelectForm(CampfireForm.Teepee));
            _cabinFormButton.button.onClick.AddListener(() => SelectForm(CampfireForm.Cabin));
            _rebuildButton.onClick.AddListener(OnRebuildClick);
        }

        private void OnRebuildClick()
        {
            bool success = _campfire.TryRebuild(_selectedForm);
            if (success) RefreshActiveForm();
            BLog.Log("CampfireUI", $"Rebuild attempted, success: {success}");
        }

        private void RefreshActiveForm()
        {
            _starFormButton.SetFormActive(_campfire.curForm == CampfireForm.Star);
            _teepeeFormButton.SetFormActive(_campfire.curForm == CampfireForm.Teepee);
            _cabinFormButton.SetFormActive(_campfire.curForm == CampfireForm.Cabin);
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
            _selectedForm = form;
        }

        private void MoveSelectorToButton(Transform button)
        {
            _formSelector.gameObject.SetActive(true);
            _formSelector.SetParent(button, worldPositionStays: false);
            _formSelector.SetAsLastSibling();
            //LayoutRebuilder.ForceRebuildLayoutImmediate(button as RectTransform); // Might need this
        }
    }
}

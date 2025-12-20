using System;
using System.Collections.Generic;
using Blizzard.Inventory;
using Blizzard.UI.Core;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using Zenject;

namespace Blizzard.Obstacles.Concrete
{
    public enum CampfireForm
    {
        Star,
        Teepee,
        Cabin
    }

    [Serializable]
    public struct CampfireFormInfo
    {
        /// <summary>
        /// Fuel units consumed per minute
        /// </summary>
        public float fuelPerMinute;
        /// <summary>
        /// Heat produced
        /// </summary>
        public float heat;
    }
    
    /// <summary>
    /// Campfire's burn mode, dictates outward appearance and heat modifier.
    /// </summary>
    internal enum BurnMode
    {
        Off,
        Low,
        High
    }
    
    public class Campfire : Structure, IInteractable
    {
        [Inject] private UIService _uiService;
        
        // TODO:
        //   Link to UI
        //   Figure out form change
        //   Have fun!
        private static readonly int FuelLevel = Animator.StringToHash("FuelLevel");

        [Header("References")] 
        [SerializeField] private Animator _animator;
        [SerializeField] private Light2D _light2D;
        [Header("Campfire Config")] 
        [SerializeField] private CampfireForm _initialForm;
        [SerializeField] private CampfireFormInfo _starFormInfo;
        [SerializeField] private CampfireFormInfo _teepeeFormInfo;
        [SerializeField] private CampfireFormInfo _cabinFormInfo;
        [SerializeField] private float _litIntensity;
        [SerializeField] private float _lowFuelLightIntensity;
        [SerializeField] private float _lowFuelHeatMultiplier;

        private Dictionary<CampfireForm, CampfireFormInfo> _campfireFormInfo;
        private CampfireForm _curForm;
        private BurnMode _curBurnMode;
        private float _mostRecentFuelConsumption = float.MinValue;
        private bool _fuelAvailable = false;
        
        // public event Action OnRebuild; TODO
        public string PrimaryInteractText => "Fuel";
        public bool PrimaryInteractReady => true;
        public readonly InventorySlot fuelSlot = new();
        public readonly InventorySlot reformSlot = new();

        private void Awake()
        {
            // Setup mapping from campfire form to info
            _campfireFormInfo = new Dictionary<CampfireForm, CampfireFormInfo>
            {
                { CampfireForm.Star, _starFormInfo },
                { CampfireForm.Teepee, _teepeeFormInfo },
                { CampfireForm.Cabin, _cabinFormInfo }
            };
            fuelSlot.OnUpdate += OnFuelSlotUpdate;
        }

        private void OnEnable()
        {
            fuelSlot.Item = null;
            reformSlot.Item = null;
            UpdateContext(BurnMode.Off, _initialForm);
        }

        private void FixedUpdate()
        {
            if (_fuelAvailable && ShouldConsumeFuel())
                ConsumeFuel();
        }
        
        public void OnPrimaryInteract()
        {
            _uiService.InitUI(UIID.Campfire, new CampfireUI.Args
            {
                campfire = this
            });
        }

        private bool ShouldConsumeFuel()
        {
            return fuelSlot.Item.id == (int)ItemID.Wood && fuelSlot.Amount > 0 &&
                   Time.time - _mostRecentFuelConsumption > 1f / (_campfireFormInfo[_curForm].fuelPerMinute / 60f);
        }


        private void OnFuelSlotUpdate()
        {
            if (_curBurnMode == BurnMode.Off)
                _fuelAvailable = IsFuelInSlot();
            else
                UpdateContext(IsFuelInSlot() ? BurnMode.High : BurnMode.Low, _curForm);
        }
        
        /// <summary>
        /// Consumes one unit of fuel. If there is none available, stops burning.
        /// </summary>
        private void ConsumeFuel()
        {
            if (!IsFuelInSlot())
            {
                UpdateContext(BurnMode.Off, _curForm);
                _fuelAvailable = false;
            }
            else
            {
                UpdateContext(fuelSlot.Amount == 1 ? BurnMode.Low : BurnMode.High, _curForm);
                fuelSlot.SetAmountQuiet(fuelSlot.Amount - 1);
                _mostRecentFuelConsumption = Time.time;
            }
        }

        private bool IsFuelInSlot()
        {
            return (!fuelSlot.Empty() && fuelSlot.Item.id == (int)ItemID.Wood);
        }
        
        /// <summary>
        /// Updates heat and visuals according to burn mode and campfire form
        /// </summary>
        private void UpdateContext(BurnMode burnMode, CampfireForm campfireForm)
        {
            if (burnMode == _curBurnMode && campfireForm == _curForm) return;
            
            _curForm = campfireForm;
            _curBurnMode = burnMode;
            switch (burnMode)
            {
                case BurnMode.Off:
                    SetHeat(0);
                    _animator.SetInteger(FuelLevel, 0);
                    break;
                case BurnMode.Low:
                    SetHeat(_campfireFormInfo[campfireForm].heat * _lowFuelHeatMultiplier);
                    _animator.SetInteger(FuelLevel, 1);
                    _light2D.intensity = _lowFuelLightIntensity;
                    break;
                case BurnMode.High:
                    SetHeat(_campfireFormInfo[campfireForm].heat);
                    _animator.SetInteger(FuelLevel, 3);
                    _light2D.intensity = _litIntensity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(burnMode), burnMode, null);
            }
        }
    }
}
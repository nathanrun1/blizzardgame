using System;
using System.Collections.Generic;
using Blizzard.Inventory;
using Blizzard.UI;
using Blizzard.UI.Core;
using Blizzard.Utilities.Logging;
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
        public ItemAmountPair rebuildCost;

        private Dictionary<CampfireForm, CampfireFormInfo> _campfireFormInfo;
        private BurnMode _curBurnMode = BurnMode.Off;
        private float _mostRecentFuelConsumption = float.MinValue;

        public string PrimaryInteractText => "Fuel";
        public bool PrimaryInteractReady => true;
        public readonly InventorySlot fuelSlot = new();
        public readonly InventorySlot rebuildSlot = new();
        public CampfireForm curForm;

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
            rebuildSlot.Item = null;
            UpdateContext(BurnMode.Off, _initialForm, force: true);
        }

        private void FixedUpdate()
        {
            if (!IsBurning())
                ConsumeFuel();
        }
        
        public void OnPrimaryInteract()
        {
            _uiService.InitUI(UIID.Campfire, new CampfireUI.Args
            {
                campfire = this
            });
        }

        
        /// <summary>
        /// Attempts to rebuild the campfire with a new form. Returns whether successful, based on whether
        /// sufficient cost provided in rebuild slot.
        /// </summary>
        public bool TryRebuild(CampfireForm form)
        {
            if (curForm == form || !rebuildSlot.Item || rebuildSlot.Item != rebuildCost.item) return false;
            
            int removed = rebuildSlot.Remove(rebuildCost.amount, false);
            if (removed < rebuildCost.amount) return false;
            
            UpdateContext(_curBurnMode, form);
            return true;
        }

        public CampfireFormInfo GetCampfireFormInfo()
        {
            return _campfireFormInfo[curForm];
        }
        
        /// <summary>
        /// Determines whether the campfire is still burning on previously consumed fuel, i.e. whether not enough
        /// time has passed since the last fuel consumption to necessitate more fuel consumption.
        /// </summary>
        /// <returns></returns>
        private bool IsBurning()
        {
            return Time.time - _mostRecentFuelConsumption <= 1f / (_campfireFormInfo[curForm].fuelPerMinute / 60f);
        }

        private void OnFuelSlotUpdate()
        {
            if (IsBurning())
                UpdateContext(IsFuelInSlot() ? BurnMode.High : BurnMode.Low, curForm);
        }
        
        /// <summary>
        /// Consumes one unit of fuel. If there is none available, stops burning.
        /// </summary>
        private void ConsumeFuel()
        {
            if (!IsFuelInSlot())
            {
                UpdateContext(BurnMode.Off, curForm);
            }
            else
            {
                _mostRecentFuelConsumption = Time.time;
                fuelSlot.Amount--;
            }
        }

        private bool IsFuelInSlot()
        {
            return (!fuelSlot.Empty() && fuelSlot.Item.id == (int)ItemID.Wood);
        }
        
        /// <summary>
        /// Updates heat and visuals according to burn mode and campfire form
        /// </summary>
        private void UpdateContext(BurnMode burnMode, CampfireForm campfireForm, bool force = true)
        {
            if (!force && burnMode == _curBurnMode && campfireForm == curForm) return;
            
            curForm = campfireForm;
            _curBurnMode = burnMode;
            switch (burnMode)
            {
                case BurnMode.Off:
                    SetHeat(0);
                    _animator.SetInteger(FuelLevel, 0);
                    _light2D.intensity = 0;
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
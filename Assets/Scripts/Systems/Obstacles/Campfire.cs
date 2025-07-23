using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;

namespace Blizzard.Obstacles.Concrete
{
    public class Campfire : Obstacle
    {
        [Header("References")]
        [SerializeField] Animator _animator;
        [SerializeField] Light2D _light2D;
        [Header("Config")]
        [SerializeField] int _fuelLevelAmount = 4;
        /// <summary>
        /// Light intensity by fuel level
        /// </summary>
        [SerializeField] float[] _lightIntensities = { 0f, .5f, .75f, 1 };
        /// <summary>
        /// Thresholds for each fuel level, corresponding to fuel/maxFuel
        /// </summary>
        [SerializeField] float[] _fuelThresholds = { 0f, .1f, .25f, .6f };
        /// <summary>
        /// Max fuel that can be added
        /// </summary>
        [SerializeField] int _maxFuel;

        private int _fuel = 0;

        private void OnValidate()
        {
            Assert.IsTrue(_lightIntensities.Length == _fuelLevelAmount, "Light Intensities array size must be equal to fuelLevelAmount!");
            Assert.IsTrue(_fuelThresholds.Length == _fuelLevelAmount, "Fuel thresholds array size must be equal to fuelLevelAmount!");
        }

        private void Start()
        {
            SetFuel(0);
        }

        [Button]
        private void SetFuel(int fuel)
        {
            _fuel = Mathf.Min(fuel, _maxFuel);
            int level = _fuelThresholds.Length - 1;
            while (level > 0)
            {
                if (((float)_fuel / (float)_maxFuel) >= _fuelThresholds[level])
                {
                    // Fuel sufficient for current level
                    break;
                }
            }
            SetFuelLevel(level);
        }

        private void SetFuelLevel(int level)
        {
            _animator.SetInteger("FuelLevel", level);
            _light2D.intensity = _lightIntensities[level];
            // TODO: other relevant shit like heat probably
        }
    }
}
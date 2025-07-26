using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;

namespace Blizzard.Obstacles.Concrete
{
    public class Campfire : Structure
    {
        [Header("References")]
        [SerializeField] Animator _animator;
        [SerializeField] Light2D _light2D;
        [Header("Campfire Config")]
        [SerializeField] int _fuelLevelAmount = 3;
        /// <summary>
        /// Light intensity by fuel level
        /// </summary>
        [SerializeField] float[] _lightIntensities = { 0f, .5f, .75f, 1 };
        /// <summary>
        /// Fuel must be higher than fuel threshold for corresponding level.
        /// fuelThresholds[i] is threshold for level i + 1.
        /// </summary>
        [SerializeField] int[] _fuelThresholds = { 0, 25, 60 };
        [SerializeField] int[] _heatLevels = { 0, 5, 7, 10 };
        /// <summary>
        /// Max fuel that can be added
        /// </summary>
        [SerializeField] int _maxFuel = 100;

        private int _fuel = 0;

        private void OnValidate()
        {
            Assert.IsTrue(_lightIntensities.Length == _fuelLevelAmount + 1, "Light Intensities array size must be equal to fuelLevelAmount + 1!");
            Assert.IsTrue(_fuelThresholds.Length == _fuelLevelAmount, "Fuel thresholds array size must be equal to fuelLevelAmount!");
            Assert.IsTrue(_heatLevels.Length == _fuelLevelAmount + 1, "Heat Levels array size must be equal to fuelLevelAmount + 1!");
        }

        private void Start()
        {
            SetFuel(0);
        }

        [Button]
        private void SetFuel(int fuel)
        {
            _fuel = Mathf.Min(fuel, _maxFuel);
            int level = _fuelThresholds.Length;

            for (; level > 0; level--)
            {
                if (_fuel > _fuelThresholds[level - 1]) break; // Sufficient fuel for this level
            }

            SetFuelLevel(level);
        }

        private void SetFuelLevel(int level)
        {
            _animator.SetInteger("FuelLevel", level);
            _light2D.intensity = _lightIntensities[level];
            SetHeat(_heatLevels[level]);
            // TODO: other relevant shit like heat probably
        }
    }
}
using Blizzard.Temperature;
using System;
using UnityEngine;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// An object that occupies a grid space in the obstacle grid
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public event Action OnDestroy;

        public float Temperature { get => _temperatureGetter(); }
        public float Heat { get; protected set; }
        public float Insulation { get; protected set; }

        [SerializeField] private float _startingInsulation = TemperatureConstants.DefaultInsulationValue;
        [SerializeField] private float _startingHeat = TemperatureConstants.DefaultHeatValue;


        private Func<float> _temperatureGetter;


        private void Awake()
        {
            this.Insulation = _startingInsulation;
            this.Heat = _startingHeat;
        }


        /// <summary>
        /// Sets delegate that retrieves the obstacle's current temperature to
        /// </summary>
        public void SetTemperatureGetter(Func<float> getter) // may be a better approach, but whatever it WORKS BRO
        {
            this._temperatureGetter = getter;
        }

        /// <summary>
        /// Destroys the obstacle.
        /// </summary>
        public void Destroy()
        {
            OnDestroy?.Invoke();
            Destroy(this.gameObject);
        }
    }
}

// "If it works, it works."

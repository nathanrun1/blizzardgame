using Blizzard.Temperature;
using ModestTree;
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
        /// <summary>
        /// Invoked whenever heat sim data has been updated.
        /// </summary>
        public event Action HeatDataUpdated;

        public float Heat { get; protected set; } = TemperatureConstants.DefaultHeatValue;
        public float Insulation { get; protected set; } = TemperatureConstants.DefaultInsulationValue;


        public void Init(float startingHeat, float startingInsulation)
        {
            Heat = startingHeat;
            Insulation = startingInsulation;
        }

        /// <summary>
        /// Sets insulation of this obstacle to given value
        /// </summary>
        protected void SetInsulation(float insulation)
        {
            Assert.That(0 <= insulation && insulation <= 1);
            this.Insulation = insulation;
            HeatDataUpdated?.Invoke();
        }

        /// <summary>
        /// Sets heat of this obstacle to given value
        /// </summary>
        protected void SetHeat(float heat)
        {
            this.Heat = heat;
            HeatDataUpdated?.Invoke();
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

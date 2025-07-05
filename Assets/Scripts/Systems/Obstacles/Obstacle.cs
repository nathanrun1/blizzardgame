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

        public float Heat { get; protected set; } = TemperatureConstants.DefaultHeatValue;
        public float Insulation { get; protected set; } = TemperatureConstants.DefaultInsulationValue;


        public virtual void Init(float startingHeat, float startingInsulation)
        {
            SetHeat(startingHeat);
            SetInsulation(startingInsulation);
        }

        /// <summary>
        /// Sets insulation of this obstacle to given value
        /// </summary>
        public void SetInsulation(float insulation)
        {
            Assert.That(0 <= insulation && insulation <= 1);
            this.Insulation = insulation;
        }

        /// <summary>
        /// Sets heat of this obstacle to given value
        /// </summary>
        public void SetHeat(float heat)
        {
            this.Heat = heat;
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

using Blizzard.Temperature;
using ModestTree;
using System;
using UnityEngine;

namespace Blizzard.Obstacles
{
    [Flags] public enum ObstacleFlags
    {
        PlayerBuilt = 1 << 0
    }

    /// <summary>
    /// An object that occupies a grid space in the obstacle grid
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public event Action OnDestroy;
        /// <summary>
        /// Invoked whenever temperature sim data has been updated.
        /// </summary>
        public event Action TemperatureDataUpdated;

        public float Heat { get; protected set; } = TemperatureConstants.DefaultHeatValue;
        public float Insulation { get; protected set; } = TemperatureConstants.DefaultInsulationValue;

        public ObstacleFlags ObstacleFlags { get; protected set; } = 0;


        public virtual void Init(ObstacleData obstacleData)
        {
            ObstacleFlags = obstacleData.obstacleFlags;
            Heat = obstacleData.startingHeat;
            Insulation = obstacleData.startingInsulation;
        }

        /// <summary>
        /// Sets insulation of this obstacle to given value
        /// </summary>
        protected void SetInsulation(float insulation)
        {
            Assert.That(0 <= insulation && insulation <= 1, "Insulation value must be between 0 and 1!");
            this.Insulation = insulation;
            TemperatureDataUpdated?.Invoke();
        }

        /// <summary>
        /// Sets heat of this obstacle to given value
        /// </summary>
        protected void SetHeat(float heat)
        {
            this.Heat = heat;
            TemperatureDataUpdated?.Invoke();
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

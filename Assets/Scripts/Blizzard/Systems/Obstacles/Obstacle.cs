using ModestTree;
using System;
using Blizzard.Constants;
using UnityEngine;
using Blizzard.Utilities.Logging;
using Blizzard.Utilities.DataTypes;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// An object that occupies a grid space in the obstacle grid
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public event Action OnDestroy;

        /// <summary>
        /// Invoked whenever the obstacle has been updated in some way that's relevant externally
        /// E.g. temperature data or obstacle flags have changed
        ///
        /// Args: (Whether ObstacleFlags was changed)
        /// </summary>
        public event Action<bool> Updated;
        
        public float Heat { get; private set; } = TemperatureConstants.DefaultHeatValue;
        public float Insulation { get; private set; } = TemperatureConstants.DefaultInsulationValue;

        public ObstacleFlags ObstacleFlags { get; private set; } = 0;


        public virtual void Initialize(ObstacleData obstacleData)
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
            Assert.That(insulation is >= 0 and <= 1, "Insulation value must be between 0 and 1!");
            Insulation = insulation;
            Updated?.Invoke(false);
        }

        /// <summary>
        /// Sets heat of this obstacle to given value
        /// </summary>
        protected void SetHeat(float heat)
        {
            Heat = heat;
            Updated?.Invoke(false);
        }

        protected void SetFlags(ObstacleFlags obstacleFlags)
        {
            ObstacleFlags = obstacleFlags;
            Updated?.Invoke(true);
        }

        /// <summary>
        /// Destroys the obstacle.
        /// </summary>
        public void Destroy()
        {
            BLog.Log("Obstacle destroyed!");
            OnDestroy?.Invoke();
            Destroy(gameObject);
        }
    }
}

// "If it works, it works."
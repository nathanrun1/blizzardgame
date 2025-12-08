using UnityEngine;

namespace Blizzard.NPCs.Concrete.Wolf
{
    [System.Serializable]
    public class WolfConfig
    {
        public float walkSpeed;
        
        /// <summary>
        /// Range at which rabbit starts to flee from player
        /// </summary>
        public float playerEnterFleeRange;
        /// <summary>
        /// Range at which rabbit stops fleeing from player
        /// </summary>
        public float playerExitFleeRange;
        /// <summary>
        /// Range of time intervals between entering idle state and starting to wander
        /// </summary>
        public Vector2 wanderPeriodRange;
        /// <summary>
        /// Range of time that a rabbit wanders for
        /// </summary>
        public Vector2 wanderTimeRange;
        /// <summary>
        /// Degrees/second that the travel direction can rotate toward its intended travel direction
        /// </summary>
        public float dirAdjustmentRate;
    }
}
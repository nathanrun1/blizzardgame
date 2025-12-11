using Blizzard.NPCs.BaseStates;
using UnityEngine;

namespace Blizzard.NPCs.Concrete.Wolf
{
    [System.Serializable]
    public class WolfConfig : PassiveNPCConfig
    {
        /// <summary>
        /// Range at which wolf can, if it chooses, become hostile to the player.
        /// </summary>
        public float engagementRange;
        /// <summary>
        /// Range at which, if another wolf is within it, the wolf is considered "in pack"
        /// </summary>
        public float packRange;
        /// <summary>
        /// Amount of other wolves such that, if there are at least this many within pack range, the wolf is
        /// considered in a pack.
        /// </summary>
        public int packSize;
        /// <summary>
        /// Curve mapping a random value between 0 and 1 to the dot product between the wander direction and
        /// the direction to the nearest wolf when wandering.
        /// </summary>
        public AnimationCurve wanderTowardPackDistribution;
        /// <summary>
        /// Minimum duration of the hostile state. Ignores exit conditions when the hostile state is entered until
        /// this duration is elapsed. 
        /// </summary>
        public float minHostileDuration;
        /// <summary>
        /// Range at which wolf is no longer hostile to player
        /// </summary>
        public float exitHostileRange;
        /// <summary>
        /// Attack damage to player
        /// </summary>
        public int attackDamage;
        /// <summary>
        /// Attacks are permitted within this range to the player
        /// </summary>
        public float attackRange;
        /// <summary>
        /// Minimum delay between attacks to the player
        /// </summary>
        public float attackDelay;
        /// <summary>
        /// How long from most recent combat engagement with the player that any wolf will default to hostility.
        /// </summary>
        public float packHostileCooldown;
    }
}
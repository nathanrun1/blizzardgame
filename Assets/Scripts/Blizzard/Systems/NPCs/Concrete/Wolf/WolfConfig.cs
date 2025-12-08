using Blizzard.NPCs.BaseStates;
using UnityEngine;

namespace Blizzard.NPCs.Concrete.Wolf
{
    [System.Serializable]
    public class WolfConfig : PassiveNPCConfig
    {
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
    }
}
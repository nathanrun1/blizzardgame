using System.Collections.Generic;
using Blizzard.Utilities.DataTypes;
using UnityEngine;

namespace Blizzard.Constants
{
    public class FXConstants
    {
        /// <summary>
        /// Distance that a game object "bounces" away from the damage source during the damage bounce
        /// tween.
        /// </summary>
        public static float DamageBounceDistance = 0.05f;

        public static readonly Dictionary<DamageFlags, Color> DamageFlashColor = new Dictionary<DamageFlags, Color>
        {
            { DamageFlags.Enemy, Color.darkRed },
            { DamageFlags.Cold, Color.darkCyan }
        };
    }
}
using System;

namespace Blizzard.Utilities.DataTypes
{
    [Flags]
    public enum ObstacleFlags
    {
        /// <summary>
        /// An obstacle built by a player
        /// </summary>
        PlayerBuilt = 1 << 0,
        /// <summary>
        /// An obstacle detectable by NPCs
        /// </summary>
        Detectable = 1 << 1,
        /// <summary>
        /// An obstacle that can be passed through
        /// </summary>
        NoCollision = 1 << 2,
        /// <summary>
        /// An obstacle that removes ambient temperature at its location
        /// </summary>
        RemovesAmbient = 1 << 3
    }
}
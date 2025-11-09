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
        /// An obstacle undetectable by NPCs
        /// </summary>
        Undetectable = 1 << 1,
        /// <summary>
        /// An obstacle that can be passed through
        /// </summary>
        NoCollision = 1 << 2
    }
}
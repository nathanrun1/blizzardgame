using System;

namespace Blizzard.Utilities.DataTypes
{
    /// <summary>
    /// Describes a source of damage
    /// </summary>
    [Flags]
    public enum DamageFlags
    {
        Player = 1 << 0,
        Enemy = 1 << 1,
        Cold = 1 << 2
    }
}
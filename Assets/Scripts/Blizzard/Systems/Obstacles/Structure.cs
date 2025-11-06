using UnityEngine;
using Blizzard.Utilities.DataTypes;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// An obstacle representing a man-made structure with health
    /// </summary>
    public class Structure : Damageable
    {
        protected override void OnDeath(DamageFlags damageFlags, Vector3 sourcePosition)
        {
            base.OnDeath(damageFlags, sourcePosition);
            Destroy();
        }
    }
}

// "Captain Robert Falcon Scott"
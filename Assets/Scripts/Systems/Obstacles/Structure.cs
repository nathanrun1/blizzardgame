using UnityEngine;
using Blizzard.Utilities;

namespace Blizzard.Obstacles
{
    /// <summary>
    /// An obstacle representing a man-made structure with health
    /// </summary>
    public class Structure : Damageable
    {
        [Header("Structure References")]
        [SerializeField] SpriteRenderer[] _spriteRenderers;

        protected override void OnDamage(int damage, DamageFlags damageFlags, Vector3 sourcePosition)
        {
            if (Health > 0)
            {
                if (damageFlags.HasFlag(DamageFlags.Enemy)) 
                    StartCoroutine(FXAssistant.TintSequenceCoroutine(_spriteRenderers, Color.red));
                StartCoroutine(FXAssistant.DamageAnim(transform, sourcePosition));
            }
        }

        protected override void OnDeath(DamageFlags damageFlags, Vector3 sourcePosition)
        {
            base.OnDeath(damageFlags, sourcePosition);
            Destroy();
        }
    }
}

// "Captain Robert Falcon Scott"

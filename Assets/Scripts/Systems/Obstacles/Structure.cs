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

        private Color[] _spriteRendererColors;

        protected virtual void Awake()
        {
            // Preserve original colors
            _spriteRendererColors = new Color[_spriteRenderers.Length];
            for (int i = 0; i < _spriteRenderers.Length; ++i)
            {
                _spriteRendererColors[i] = _spriteRenderers[i].color;
            }
        }

        protected override void OnDamage(int damage)
        {
            base.OnDamage(damage);
            if (Health > 0) StartCoroutine(FXAssistant.TintSequenceCoroutine(_spriteRenderers, Color.red));
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            Destroy();
        }
    }
}

// "Captain Robert Falcon Scott"

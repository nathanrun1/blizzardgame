using System;
using System.Collections;
using Blizzard.Constants;
using DG.Tweening;
using UnityEngine;

namespace Blizzard.Utilities.Assistants
{
    /// <summary>
    /// Helper class for effects
    /// </summary>
    public static class FXAssistant
    {
        /// <summary>
        /// Applies an immediate color tint to given sprite renderers for a given duration,
        /// and then resets them to the color they were before the tint began
        /// </summary>
        public static Sequence DOColorTint(SpriteRenderer[] spriteRenderers, Color tintColor, float duration = 0.2f)
        {
            var sequence = DOTween.Sequence();
            for (int i = 0; i < spriteRenderers.Length; ++i)
            {
                Color initialColor = spriteRenderers[i].color;
                int ind = i;
                var colorSequence = DOTween.Sequence();
                
                // Set to target color
                colorSequence.Append(DOTween.To(
                    () => spriteRenderers[ind].color,
                    (color) =>
                    {
                        spriteRenderers[ind].color = color;
                    },
                    tintColor,
                    0.0f
                ));
                // Reset to initial color after given duration
                colorSequence.Append(DOTween.To(
                    () => spriteRenderers[ind].color,
                    (color) =>
                    {
                        spriteRenderers[ind].color = color;
                    },
                    initialColor,
                    duration
                ));
                colorSequence.SetLink(spriteRenderers[ind].gameObject);
                sequence.Join(colorSequence);  // Join to main sequence
            }

            return sequence;
        }

        /// <summary>
        /// Applies an immediate color tint to given sprite renderer for a given duration,
        /// and then resets it to the color it was before the tint began
        /// </summary>
        public static Sequence DOColorTint(SpriteRenderer spriteRenderer, Color tintColor, float duration = 0.2f)
        {
            return DOColorTint(new[] {spriteRenderer}, tintColor, duration);
        }

        /// <summary>
        /// A "damage" animation applied to a transform that has it bounce away from the damage source.
        /// </summary>
        /// <param name="transform">Transform of the object to animate</param>
        /// <param name="sourcePosition">Damage source position</param>
        /// <param name="duration">Duration of animation</param>
        public static Sequence DODamageBounce(Transform transform, Vector3 sourcePosition, float duration = 0.2f)
        {
            Vector3 hitDirection = (transform.position - sourcePosition).normalized;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + hitDirection * FXConstants.DamageBounceDistance;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(endPos, duration / 2f));
            sequence.Append(transform.DOMove(startPos, duration / 2f));

            return sequence;
        }
    }
}
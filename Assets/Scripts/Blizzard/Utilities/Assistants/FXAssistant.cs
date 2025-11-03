using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Blizzard.Utilities.Assistants
{
    /// <summary>
    /// Helper class for effects
    /// </summary>
    public static class FXAssistant
    {
        public static void TintSequence(SpriteRenderer[] spriteRenderers, Color targetColor, float duration = 0.2f)
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
                    targetColor,
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

            sequence.Play();
        }
        
        /// <summary>
        /// Applies an immediate red tint to given sprite renderers for a given duration,
        /// and then resets them to their original color.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator TintSequenceCoroutine(SpriteRenderer[] spriteRenderers, Color targetColor,
            float duration = 0.2f)
        {
            var initialColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; ++i)
            {
                initialColors[i] = spriteRenderers[i].color;
                spriteRenderers[i].color *= targetColor;
            }

            yield return new WaitForSeconds(duration);

            for (int i = 0; i < spriteRenderers.Length; ++i) spriteRenderers[i].color = initialColors[i];
        }

        public static IEnumerator TintSequenceCoroutine(SpriteRenderer spriteRenderer, Color targetColor,
            float duration = 0.2f)
        {
            return TintSequenceCoroutine(new[] { spriteRenderer }, targetColor, duration);
        }

        /// <summary>
        /// Applies a "damage" animation to an object that has it bounce away from the damage source.
        /// </summary>
        /// <param name="transform">Transform of the object to animate</param>
        /// <param name="sourcePosition">Damage source position</param>
        public static void DamageAnim(Transform transform, Vector3 sourcePosition)
        {
            var hitDirection = (transform.position - sourcePosition).normalized;
            var startPos = transform.position;
            var endPos = startPos + hitDirection * 0.05f;
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(endPos, 0.1f));
            sequence.Append(transform.DOMove(startPos, 0.1f));

            sequence.SetLink(transform.gameObject);

            sequence.Play();
        }

        // /// <summary>
        // /// Coroutine that applies a "damage" animation to an object
        // /// that has it bounce away from the damage source.
        // /// </summary>
        // /// <param name="transform">Transform of the object to animate</param>
        // /// <param name="sourcePosition">Damage source position</param>
        // /// <param name="breakCondition">If result is true, ends the animation early</param>
        // public static IEnumerator DamageAnim(Transform transform, Vector3 sourcePosition, Func<bool> breakCondition)
        // {
        //     if (breakCondition()) yield break;
        //
        //     var hitDirection = (transform.position - sourcePosition).normalized;
        //     var startPos = transform.position;
        //     var endPos = startPos + hitDirection * 0.05f;
        //     var sequence = DOTween.Sequence();
        //     sequence.Append(transform.DOMove(endPos, 0.1f));
        //     sequence.Append(transform.DOMove(startPos, 0.1f));
        //
        //     sequence.Play();
        //     yield return null;
        // }
    }
}
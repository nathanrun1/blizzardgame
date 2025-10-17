using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Blizzard.Utilities
{
    /// <summary>
    /// Helper class for effects
    /// </summary>
    public static class FXAssistant
    {
        /// <summary>
        /// Applies an immediate red tint to given sprite renderers for a given duration,
        /// and then resets them to their original color.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator TintSequenceCoroutine(SpriteRenderer[] spriteRenderers, Color targetColor, float duration = 0.2f)
        {
            Color[] initialColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; ++i)
            {
                initialColors[i] = spriteRenderers[i].color;
                spriteRenderers[i].color *= targetColor;
            }
            yield return new WaitForSeconds(duration);
            
            for (int i = 0; i < spriteRenderers.Length; ++i)
            {
                spriteRenderers[i].color = initialColors[i];
            }
        }

        /// <summary>
        /// Coroutine that applies a "damage" animation to an object
        /// that has it bounce away from the damage source.
        /// </summary>
        /// <param name="transform">Transform of the object to animate</param>
        /// <param name="sourcePosition">Damage source position</param>
        public static IEnumerator DamageAnim(Transform transform, Vector3 sourcePosition)
        {
            Vector3 hitDirection = (transform.position - sourcePosition).normalized;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + hitDirection * 0.05f;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(endPos, 0.1f));
            sequence.Append(transform.DOMove(startPos, 0.1f));

            sequence.Play();
            yield return null;
        }
        
        /// <summary>
        /// Coroutine that applies a "damage" animation to an object
        /// that has it bounce away from the damage source.
        /// </summary>
        /// <param name="transform">Transform of the object to animate</param>
        /// <param name="sourcePosition">Damage source position</param>
        /// <param name="breakCondition">If result is true, ends the animation early</param>
        public static IEnumerator DamageAnim(Transform transform, Vector3 sourcePosition, Func<bool> breakCondition)
        {
            if (breakCondition()) yield break;
            
            Vector3 hitDirection = (transform.position - sourcePosition).normalized;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + hitDirection * 0.05f;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(endPos, 0.1f));
            sequence.Append(transform.DOMove(startPos, 0.1f));

            sequence.Play();
            yield return null;
        }
    }
}

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
    }
}

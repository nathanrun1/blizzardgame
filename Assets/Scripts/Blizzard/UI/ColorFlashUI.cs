using System;
using Blizzard.UI.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Blizzard.UI
{
    /// <summary>
    /// Flashes a color over the entire screen
    /// </summary>
    public class ColorFlashUI : UIBase
    {
        public struct Args
        {
            /// <summary>
            /// Flash color, ensure alpha is 1
            /// </summary>
            public Color color;
            /// <summary>
            /// Flash duration
            /// </summary>
            public float duration;
        }
        
        [Header("References")]
        [SerializeField] private Image _colorFlashImage;
        /// <summary>
        /// Maximum alpha value during flash
        /// </summary>
        [Header("Config")]
        [SerializeField] private float _maxAlpha = 20f / 255f;


        private Tween _flashInTween;
        private Tween _flashOutTween;
        private Sequence _flashSequence;
        
        public override void Setup(object args)
        {
            Args colorFlashArgs;
            try
            {
                colorFlashArgs = (Args)args;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Incorrect argument type given!");
            }
            
            DoFlash(colorFlashArgs);
        }

        private void DoFlash(Args colorFlashArgs)
        {
            if (_flashSequence != null && _flashSequence.IsPlaying()) _flashSequence.Kill();  // Reset flash if currently playing
            // Flash color by tweening alpha value from 0 to _maxAlpha and then back to 0
            Color midColor = colorFlashArgs.color * new Color(1f, 1f, 1f, _maxAlpha);
            Color endColor = colorFlashArgs.color * new Color(1f, 1f, 1f, 0f);
            _colorFlashImage.color = endColor; 
            
            _flashInTween = DOTween.To(
                () => _colorFlashImage.color,
                (value) => _colorFlashImage.color = value,
                midColor,
                colorFlashArgs.duration / 2f
            );
            _flashOutTween = DOTween.To(
                () => _colorFlashImage.color,
                (value) => _colorFlashImage.color = value,
                endColor,
                colorFlashArgs.duration / 2f
            );
            
            _flashSequence = DOTween.Sequence();
            _flashSequence.Append(_flashInTween);
            _flashSequence.Append(_flashOutTween);
            _flashSequence.Play();
        }
    }
}
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
    public class ColorOverlayUI : UIBase
    {
        [Header("References")]
        [SerializeField] private Image _colorFlashImage;

        public override void Setup(object args)
        {
        }

        public void SetColor(Color color)
        {
            _colorFlashImage.color = color;
        }
    }
}
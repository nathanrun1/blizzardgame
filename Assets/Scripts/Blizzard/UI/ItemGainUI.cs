using ModestTree;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Blizzard.Inventory;
using Blizzard.UI.Core;


namespace Blizzard.UI
{
    /// <summary>
    /// UI that briefly appears on the screen to showcase the player collecting an item
    /// </summary>
    public class ItemGainUI : UIBase
    {
        public struct Args
        {
            public ItemData item;
            public int amount;
            /// <summary>
            /// World position to display the UI at
            /// </summary>
            public Vector3 worldPosition;

            /// <summary>
            /// Invoked when animation is completed
            /// </summary>
            public Action OnAnimComplete;
        }

        [Header("GameObject References")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _count;

        [Header("Anim config")]
        [SerializeField] private float _animYDelta = 30f;
        [SerializeField] private float _animLength = 0.2f;
        /// <summary>
        /// Color of number when item is removed
        /// </summary>
        [SerializeField] private Color _negativeColor = new Color32(217, 5, 5, 255);

        public override void Setup(object args)
        {
            Args itemGainArgs;
            try
            {
                itemGainArgs = (Args)args;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Incorrect argument type given!");
            }

            if (itemGainArgs.amount == 0) Close(); // Only display non-zero amount

            transform.localPosition = GetUILocalPos(itemGainArgs.worldPosition);
            GainItemAnim(itemGainArgs.item, itemGainArgs.amount, itemGainArgs.OnAnimComplete);
        }

        /// <summary>
        /// Converts world position of where item was collected to UI's local position
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        private Vector2 GetUILocalPos(Vector3 worldPosition)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            Vector2 screenPosRatio = new Vector2(screenPos.x / Camera.main.pixelWidth, screenPos.y / Camera.main.pixelHeight);

            // Assumes parent spans entire screen (it should be main canvas so likely valid assumption)
            Vector2 localPos = new Vector2(
                screenPosRatio.x * _parent.GetComponent<RectTransform>().rect.width - (_parent.GetComponent<RectTransform>().rect.width / 2),
                screenPosRatio.y * _parent.GetComponent<RectTransform>().rect.height - (_parent.GetComponent<RectTransform>().rect.height) / 2);

            return localPos;
        }

        private void GainItemAnim(ItemData item, int amount, Action onComplete)
        {
            switch (amount)
            {
                case > 0:
                {
                    // Positive
                    _icon.gameObject.SetActive(true);
                    _count.gameObject.SetActive(true);

                    _icon.sprite = item.icon;
                    _count.text = $"+{amount}";
                    _count.color = item.itemGainColor;
                    Sequence seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMoveY(transform.localPosition.y + _animYDelta, _animLength));
                    Color initialIconColor = _icon.color;
                    Color initialCountColor = _count.color;
                    //seq.Join(_icon.DOColor(new Color(initialIconColor.r, initialIconColor.g, initialIconColor.b, 0), ANIM_LENGTH));
                    //seq.Join(_icon.DOColor(new Color(initialCountColor.r, initialCountColor.g, initialCountColor.b, 0), ANIM_LENGTH));
                    seq.OnComplete(() =>
                    {
                        onComplete?.Invoke();
                        Close();
                    });
                    seq.Play();
                    break;
                }
                case < 0:
                {
                    // Negative
                    _icon.gameObject.SetActive(true);
                    _count.gameObject.SetActive(true);

                    _icon.sprite = item.icon;
                    _count.text = $"-{-amount}";
                    _count.color = _negativeColor;
                    Sequence seq = DOTween.Sequence();
                    transform.localPosition += new Vector3(0, _animYDelta, 0);
                    seq.Append(transform.DOLocalMoveY(transform.localPosition.y - _animYDelta, _animLength));
                    Color initialIconColor = _icon.color;
                    Color initialCountColor = _count.color;
                    //seq.Join(_icon.DOColor(new Color(initialIconColor.r, initialIconColor.g, initialIconColor.b, 0), ANIM_LENGTH));
                    //seq.Join(_icon.DOColor(new Color(initialCountColor.r, initialCountColor.g, initialCountColor.b, 0), ANIM_LENGTH));
                    seq.OnComplete(() =>
                    {
                        onComplete?.Invoke();
                        Close();
                    });
                    seq.Play();
                    break;
                }
            }
        }
    }
}

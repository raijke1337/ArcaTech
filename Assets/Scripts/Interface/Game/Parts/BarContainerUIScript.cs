using Arcatech.Stats;
using DG.Tweening;
using KBCore.Refs;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class BarContainerUIScript : ValidatedMonoBehaviour
    {
        [Header("Prefab settings")]
        [SerializeField] private Image _fill;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField, Self] Image _background;

        private ColorSet _colors;
        Color _baseBgColor;
        private Ease _ease = Ease.Linear;
        float fillTime = 0.1f;
        public BarContainerUIScript SetColors(ColorSet color)
        {
            _baseBgColor = _background.color;
            _colors = color;
            _fill.color = _colors.BaseColor;

            return this;
        }
        public BarContainerUIScript SetEaseMethod(Ease e)
        {
            _ease = e;
            return this;
        }
        public BarContainerUIScript SetFillTime(float time)
        {
            fillTime = time;
            return this;
        }

        public void UpdateValue(StatValueContainer c)
        {
            Color flash = new Color(0, 0, 0, 0); // transparent white
            if (c.GetCurrent > c.CachedValue) // heal
                {
                    flash = _colors.PositiveColor;
                }
            if (c.GetCurrent < c.CachedValue)
                {
                    flash = _colors.NegativeColor;
                }

            _background.DOColor(flash, 0.1f).SetEase(Ease.InQuint).Play().onComplete += () => _background.DOColor(_baseBgColor, 0.1f).SetEase(Ease.InQuint).Play(); 
            try
            {
                _fill.DOFillAmount(c.GetPercent, fillTime).SetEase(_ease).Play();
                _text.text = c.ToString();
            }

            catch { }
        }

    }
}
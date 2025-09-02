using Arcatech.Stats;
using DG.Tweening;
using KBCore.Refs;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class StatBarContainerUIScript : ValidatedMonoBehaviour
    {
        [Header("Prefab settings")]
        [SerializeField] private Image _fill;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField, Self] Image _background;

        private ColorSet _colors;
        Color _baseBgColor;
        private Ease _ease = Ease.Linear;
        float fillTime = 0.1f;
        StatValueContainer _valueContainer;
        float deltaTreschold = 1;

        #region setup
        public StatBarContainerUIScript LinkContainer(StatValueContainer c)
        {
            _valueContainer = c;
            return this;    
        }
        public StatBarContainerUIScript SetColors(ColorSet color)
        {
            _baseBgColor = _background.color;
            _colors = color;
            _fill.color = _colors.BaseColor;

            return this;
        }
        public StatBarContainerUIScript SetEaseMethod(Ease e)
        {
            _ease = e;
            return this;
        }
        public StatBarContainerUIScript SetFillTime(float time)
        {
            fillTime = time;
            return this;
        }
        
        public StatBarContainerUIScript SetShake(float deltaTr)
        {
            deltaTreschold = deltaTr;
            return this;
        }
        #endregion

        private void Update()
        {
            

            if (_valueContainer != null && _valueContainer.Initialized)
            {
                _fill.DOFillAmount(_valueContainer.GetPercent, fillTime).SetEase(_ease).Play();
                _text.text = _valueContainer.ToString();

                if (_valueContainer.GetFrameDeltaPercentAbs > deltaTreschold)
                {
                    var delta = _valueContainer.GetFrameDeltaValue;
                    if (delta != 0)
                    {
                        Color flash = new Color(0, 0, 0, 0);
                        flash = delta > 0 ? _colors.PositiveColor : _colors.NegativeColor;
                        _background.DOColor(flash, 0.1f).SetEase(Ease.InQuint).Play().
                    onComplete += () => _background.DOColor(_baseBgColor, 0.1f).SetEase(Ease.InQuint).Play();
                    }
                }           
            }
        }

    }
}
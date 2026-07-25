using DG.Tweening;
using KBCore.Refs;
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
        float deltaTreschold = 1;
        
        #region setup

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
        
        public StatBarContainerUIScript SetBrightGlowAT(float deltaTr)
        {
            deltaTreschold = deltaTr;
            return this;
        }
        #endregion


        public void UpdateValue(float statCurrent, float statMax, float statDelta)
        {
            _fill.DOFillAmount(statCurrent/statMax, fillTime).SetEase(_ease).Play();
            _text.text = ($"{Mathf.RoundToInt(statCurrent)}  /  {statMax}");

            if (statDelta!= 0 && statDelta/statMax > deltaTreschold)
            {
                Color flash = new Color(0, 0, 0, 0);
                flash = statDelta > 0 ? _colors.PositiveColor : _colors.NegativeColor;
                _background.DOColor(flash, 0.1f).SetEase(Ease.InQuint).Play().
                    onComplete += () => _background.DOColor(_baseBgColor, 0.1f).SetEase(Ease.InQuint).Play();
            }
        }

        public void DrawShield(float value)
        {
            
        }
    }
}
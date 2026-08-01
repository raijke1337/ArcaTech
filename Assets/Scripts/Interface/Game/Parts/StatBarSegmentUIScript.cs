using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{

    public class StatBarSegmentUIScript : MonoBehaviour
    {
        [SerializeField] private Image _fill;
        [SerializeField] private Image _background;

        private Tween _blinkTween;
        private Color _fillBaseColor;
        private Color _backgroundBaseColor;

        private void Awake()
        {
            _fillBaseColor = _fill.color;
            _backgroundBaseColor = _background.color;
        }

        private void OnDestroy()
        {
            _blinkTween?.Kill();
        }

        public void SetColors(Color fillColor, Color backgroundColor)
        {
            _fillBaseColor = fillColor;
            _backgroundBaseColor = backgroundColor;

            _fill.color = _fillBaseColor;
            _background.color = _backgroundBaseColor;
        }

        public void SetFill(float normalizedValue)
        {
            _fill.fillAmount = Mathf.Clamp01(normalizedValue);
        }

        public void SetEmpty()
        {
            StopBlink();

            _fill.fillAmount = 0f;
            _fill.color = _fillBaseColor;
        }

        public void SetFull()
        {
            StopBlink();

            _fill.fillAmount = 1f;
            _fill.color = _fillBaseColor;
        }

        public void SetPartialAndBlink(float normalizedValue, float blinkDuration)
        {
            _fill.fillAmount = Mathf.Clamp01(normalizedValue);

            StartBlink(blinkDuration);
        }

        private void StartBlink(float blinkDuration)
        {
            if (_blinkTween != null && _blinkTween.IsActive())
            {
                return;
            }

            Color visibleColor = _fillBaseColor;
            Color dimColor = _fillBaseColor;
            dimColor.a = 0.3f;

            _fill.color = visibleColor;

            _blinkTween = _fill
                .DOColor(dimColor, blinkDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopBlink()
        {
            _blinkTween?.Kill();
            _blinkTween = null;

            _fill.color = _fillBaseColor;
        }
    }
}
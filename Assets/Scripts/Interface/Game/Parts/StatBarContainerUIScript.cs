using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class StatBarContainerUIScript : ValidatedMonoBehaviour
    {
        private const float HpPerSegment = 10f;

        [Header("Prefab settings")] [SerializeField]
        private StatBarSegmentUIScript _segmentPrefab;

        [SerializeField] private Transform _segmentsRoot;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image pictogram;
        [SerializeField, Self] private Image _background;

        [Header("Animation settings")] [SerializeField]
        private float _fillTime = 0.1f;

        [SerializeField] private float _blinkDuration = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _deltaThreshold = 0.05f;

        private readonly List<StatBarSegmentUIScript> _segments = new();

        private ColorSet _colors;
        private Color _baseBgColor;

        private Ease _ease = Ease.Linear;

        private float _displayedHp;
        private float _lastMaxHp = -1f;

        private Tween _hpTween;
        private Tween _backgroundTween;

        #region Setup

        public StatBarContainerUIScript SetColors(ColorSet color)
        {
            _baseBgColor = _background.color;
            _colors = color;

            UpdateSegmentColors();

            nameText.text = color.ShortName;
            pictogram.sprite = color.Pictogram;
            return this;
        }

        public StatBarContainerUIScript SetEaseMethod(Ease ease)
        {
            _ease = ease;
            return this;
        }

        public StatBarContainerUIScript SetFillTime(float time)
        {
            _fillTime = Mathf.Max(0f, time);
            return this;
        }

        public StatBarContainerUIScript SetBrightGlowAT(float deltaThreshold)
        {
            _deltaThreshold = Mathf.Clamp01(deltaThreshold);
            return this;
        }

        #endregion

        private void OnDestroy()
        {
            _hpTween?.Kill();
            _backgroundTween?.Kill();
        }

        public void UpdateValue(float statCurrent, float statMax, float statDelta)
        {
            if (statMax <= 0f)
            {
                Debug.LogWarning($"{name}: Max HP must be greater than zero.");
                return;
            }

            statCurrent = Mathf.Clamp(statCurrent, 0f, statMax);

            EnsureSegmentCount(statMax);

            valueText.text = $"{Mathf.CeilToInt(statCurrent)}";

            AnimateHp(statCurrent, statMax);

            float normalizedDelta = Mathf.Abs(statDelta) / statMax;

            if (statDelta != 0f && normalizedDelta >= _deltaThreshold)
            {
                Color flashColor = statDelta > 0f
                    ? _colors.PositiveColor
                    : _colors.NegativeColor;

                FlashBackground(flashColor);
            }
        }

        private void EnsureSegmentCount(float maxHp)
        {
            int requiredSegmentCount = Mathf.CeilToInt(maxHp / HpPerSegment);

            if (_segments.Count == requiredSegmentCount && Mathf.Approximately(_lastMaxHp, maxHp))
            {
                return;
            }

            _lastMaxHp = maxHp;

            ClearSegments();

            for (int i = 0; i < requiredSegmentCount; i++)
            {
                StatBarSegmentUIScript segment =
                    Instantiate(_segmentPrefab, _segmentsRoot);

                _segments.Add(segment);
            }

            UpdateSegmentColors();
        }

        private void ClearSegments()
        {
            foreach (StatBarSegmentUIScript segment in _segments)
            {
                if (segment != null)
                {
                    Destroy(segment.gameObject);
                }
            }

            _segments.Clear();
        }

        private void UpdateSegmentColors()
        {
            if (_colors == null)
            {
                return;
            }

            foreach (StatBarSegmentUIScript segment in _segments)
            {
                segment.SetColors(
                    _colors.BaseColor,
                    new Color(
                        _colors.BaseColor.r,
                        _colors.BaseColor.g,
                        _colors.BaseColor.b,
                        0.12f
                    )
                );
            }
        }

        private void AnimateHp(float targetHp, float maxHp)
        {
            _hpTween?.Kill();

            _hpTween = DOTween
                .To(
                    () => _displayedHp,
                    value =>
                    {
                        _displayedHp = value;
                        DrawSegments(value, maxHp);
                    },
                    targetHp,
                    _fillTime
                )
                .SetEase(_ease);
        }

        private void DrawSegments(float currentHp, float maxHp)
        {
            currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

            int fullSegments = Mathf.FloorToInt(currentHp / HpPerSegment);
            float remainder = currentHp % HpPerSegment;

            bool hasPartialSegment =
                remainder > 0.01f &&
                fullSegments < _segments.Count;

            for (int i = 0; i < _segments.Count; i++)
            {
                if (i < fullSegments)
                {
                    _segments[i].SetFull();
                    continue;
                }

                if (i == fullSegments && hasPartialSegment)
                {
                    float partialFill = remainder / HpPerSegment;

                    _segments[i].SetPartialAndBlink(
                        partialFill,
                        _blinkDuration
                    );

                    continue;
                }

                _segments[i].SetEmpty();
            }
        }

        private void FlashBackground(Color flashColor)
        {
            _backgroundTween?.Kill();

            _backgroundTween = DOTween.Sequence()
                .Append(
                    _background
                        .DOColor(flashColor, 0.08f)
                        .SetEase(Ease.OutQuad)
                )
                .Append(
                    _background
                        .DOColor(_baseBgColor, 0.15f)
                        .SetEase(Ease.InQuad)
                );
        }

        public void DrawShield(float value)
        {
            // Щит лучше сделать отдельным рядом сегментов
            // или отдельным экземпляром StatBarContainerUIScript.
        }
    }
}
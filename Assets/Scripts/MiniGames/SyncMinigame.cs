using System;
using UnityEngine;
using UnityEngine.UI;
using Arcatech.Interactions;
using DG.Tweening;
using TMPro;

namespace Arcatech.MiniGames
{
    public class RhythmSyncMinigame : MiniGameBase
    {
        [Header("UI References")]
        public Slider progressSlider; 
        public RectTransform targetWave; 
        public RectTransform playerWave; 
        
        
        [Header("Settings")]
        public float targetFrequency = 1.0f; 
        public float playerFrequencyBase = 0.8f; 
        public float syncTolerance = 0.1f; 
        public float progressPerHit = 0.2f; 
        public float penaltyPerMiss = 0.1f; 

        [Header("Input & Cooldown")]
        [Tooltip("Время в секундах, в течение которого нельзя нажать кнопку снова после любого нажатия")]
        public float inputCooldown = 0.4f; 
        
        [Header("Lose conditions")] 
        [SerializeField] private bool timeExpires = false;
        [SerializeField, Range(1f, 60f)] private float timeLimit = 10f;
        [SerializeField] private TextMeshProUGUI timerText;
        
        [Space]
        [SerializeField] private bool limitedFailures = false;
        [SerializeField, Range(1, 10)] private int allowedFailures = 3;
        [SerializeField] private TextMeshProUGUI attemptsText;

        [Header("Accessibility & Feedback")]
        public bool reduceMotion = false;
        
        private float _currentTime;
        private int _currentFails;
        private int _currentSessionId;
        private Tween _sliderTween;
        
        // Новая переменная для отслеживания отката
        private float _currentCooldown = 0f;
        private Image _playerWaveImage; // Кэш компонента Image для визуального фидбека

        protected override void OnGameStarted()
        {
            _currentSessionId = SessionId;
            _currentTime = 0f;
            _currentFails = 0;
            _currentCooldown = 0f;

            if (playerWave != null)
            {
                _playerWaveImage = playerWave.GetComponent<Image>();
            }

            if (timeExpires && timerText != null)
            {
                timerText.gameObject.SetActive(true);
                timerText.text = timeLimit.ToString("F1"); 
            }

            if (limitedFailures && attemptsText != null)
            {
                attemptsText.gameObject.SetActive(true);
                attemptsText.text = allowedFailures.ToString();
            }
            
            UpdateWaves();
        }

        protected override void OnGameEnded()
        {
            _sliderTween?.Kill(); 
            
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (attemptsText != null) attemptsText.gameObject.SetActive(false);
        }

        public override void ResetGame()
        {
            _sliderTween?.Kill();
            if (progressSlider != null) progressSlider.value = 0f;
            _currentTime = 0f;
            _currentFails = 0;
            _currentCooldown = 0f; // Сброс кулдауна
        }

        void Update()
        {
            if (!IsRunning || IsFinishing || SessionId != _currentSessionId) return;

            _currentTime += Time.deltaTime;
            
            // Обработка отката ввода
            if (_currentCooldown > 0f)
            {
                _currentCooldown -= Time.deltaTime;
            }

            if (timeExpires)
            {
                float remainingTime = Mathf.Max(0f, timeLimit - _currentTime);
                if (timerText != null)
                {
                    timerText.text = remainingTime.ToString("F1");
                    if (remainingTime <= 3f)
                    {
                        timerText.color = ColorUtility.TryParseHtmlString("#FF5268", out var red) ? red : Color.red;
                    }
                }

                if (_currentTime >= timeLimit)
                {
                    ReportResult(InteractionState.Failure);
                }
            }
            
            UpdateWaves();
        }

        void UpdateWaves()
        {
            float targetScale = 1f + Mathf.Sin(_currentTime * targetFrequency) * 0.2f;
            float playerScale = 1f + Mathf.Sin(_currentTime * playerFrequencyBase) * 0.2f;

            if (targetWave != null) targetWave.localScale = Vector3.one * targetScale;

            if (playerWave != null)
            {
                playerWave.localScale = Vector3.one * playerScale;

                // === ВИЗУАЛЬНЫЙ ФИДБЕК КУЛДАУНА ===
                if (_playerWaveImage != null)
                {
                    if (_currentCooldown > 0f)
                    {
                        // Во время отката волна становится серой/заблокированной (#566174)
                        Color targetColor = ColorUtility.TryParseHtmlString("#566174", out var c) ? c : Color.gray;
                        _playerWaveImage.color = Color.Lerp(_playerWaveImage.color, targetColor, Time.deltaTime * 15f);
                    }
                    else
                    {
                        // В обычном состоянии волна розово-магентовая (#FF4FA3)
                        Color targetColor = ColorUtility.TryParseHtmlString("#FF4FA3", out var c) ? c : Color.magenta;
                        _playerWaveImage.color = Color.Lerp(_playerWaveImage.color, targetColor, Time.deltaTime * 15f);
                    }
                }
            }
        }

        public void CheckSync()
        {
            // ИГНОРИРУЕМ ВВОД, ЕСЛИ ИДЕТ ОТКАТ
            if (_currentCooldown > 0f) return;

            if (!IsRunning || IsFinishing || SessionId != _currentSessionId) return;

            float targetPhase = Mathf.Sin(_currentTime * targetFrequency);
            float playerPhase = Mathf.Sin(_currentTime * playerFrequencyBase);
            
            float currentValue = progressSlider != null ? progressSlider.value : 0f;
            float newValue;
            
            if (Mathf.Abs(targetPhase - playerPhase) < syncTolerance)
            {
                // === УСПЕХ ===
                newValue = Mathf.Min(1f, currentValue + progressPerHit);
                
                // ЗАПУСКАЕМ КУЛДАУН
                _currentCooldown = inputCooldown;

                ApplySliderTween(newValue, () => {
                    if (newValue >= 1f)
                    {
                        ReportResult(InteractionState.Success);
                    }
                });
            }
            else
            {
                // === ПРОМАХ ===
                // Кулдаун ставится и на промах, чтобы игрок не мог спамить кнопку в панике
                _currentCooldown = inputCooldown;

                if (limitedFailures)
                {
                    _currentFails++;
                    if (attemptsText != null)
                    {
                        attemptsText.text = Mathf.Max(0, allowedFailures - _currentFails).ToString();
                        attemptsText.color = ColorUtility.TryParseHtmlString("#FF5268", out var red) ? red : Color.red;
                    }

                    if (_currentFails >= allowedFailures)
                    {
                        ReportResult(InteractionState.Failure);
                        return; 
                    }
                }

                newValue = Mathf.Max(0f, currentValue - penaltyPerMiss);
                ApplySliderTween(newValue, () => {
                    if (newValue <= 0f)
                    {
                        ReportResult(InteractionState.Failure);
                    }
                });
            }
        }

        private void ApplySliderTween(float targetValue, Action onComplete)
        {
            if (progressSlider == null)
            {
                onComplete?.Invoke();
                return;
            }

            _sliderTween?.Kill();

            if (reduceMotion)
            {
                progressSlider.value = targetValue;
                onComplete?.Invoke();
            }
            else
            {
                _sliderTween = progressSlider.DOValue(targetValue, 0.2f).SetEase(Ease.OutQuad);
                _sliderTween.onComplete += () => onComplete?.Invoke();
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using Arcatech.Interactions;
using Arcatech.Managers;
using TMPro;
using UnityEngine.InputSystem;

namespace Arcatech.MiniGames
{
    public class OverloadStabilizerMinigame : MiniGameBase
    {
        [Header("UI References")]
        public Slider markerSlider; // Ползунок маркера
        public RectTransform safeZoneRect; // Зеленая зона
        public Image markerImage; // Визуальный образ маркера
        public Slider progressSlider;

        [Header("Settings")]
        public float baseSpeed = 1.5f; // Скорость движения маркера
        public float holdSlowFactor = 0.2f; // Насколько замедляется маркер при удержании
        public float safeZoneWidth = 0.15f; // Ширина зоны в % (0-1)
        public float fillRate = 0.5f; // Скорость заполнения прогресса
        public float drainRate = 1.0f; // Скорость падения прогресса вне зоны
        public MiniGameButton buttonToActivate = MiniGameButton.ButtonS;

        [Header("Lose Conditions")]
        [SerializeField] private bool timeExpires = false;
        [SerializeField, Range(1f, 60f)] private float timeLimit = 15f;
        [SerializeField] private TextMeshProUGUI timerText;

        [Space]
        [SerializeField] private bool limitedFailures = false;
        [SerializeField, Range(1, 10)] private int allowedFailures = 3;
        [SerializeField] private TextMeshProUGUI attemptsText;

        [Header("Accessibility")]
        public bool reduceMotion = false;

        // === Private State ===
        private int _currentSessionId;
        private float _currentTime;
        private int _currentFails;
        private bool _isHoldingInput;

        protected override void OnGameStarted()
        {
            _currentSessionId = SessionId;
            _currentTime = 0f;
            _currentFails = 0;
            _isHoldingInput = false;
            progressSlider.value = 0;
            if (markerSlider != null) markerSlider.value = 0.5f;

            UpdateUI();
        }

        protected override void OnGameEnded()
        {
            _isHoldingInput = false;

            if (timerText != null) timerText.gameObject.SetActive(false);
            if (attemptsText != null) attemptsText.gameObject.SetActive(false);
        }

        protected override void HandleButtonPress(MiniGameButton button, InputAction.CallbackContext context)
        {
            if (button != buttonToActivate) return;

            // Button-интеракция вызывает callback на started (нажали) и canceled (отпустили).
            // started может прийти вместе с performed — держим состояние по обеим границам,
            // а не полагаемся на конкретную фазу.
            if (context.started)
            {
                _isHoldingInput = true;
            }
            else if (context.canceled)
            {
                _isHoldingInput = false;
            }
        }

        public override void ResetGame()
        {
            _currentTime = 0f;
            _currentFails = 0;
            _isHoldingInput = false;
            if (markerSlider != null) markerSlider.value = 0.5f;
        }


        void Update()
        {
            if (!IsRunning || IsFinishing || SessionId != _currentSessionId) return;

            _currentTime += Time.deltaTime;

            // === Marker Movement ===
            if (markerSlider != null)
            {
                // Реализация "дрейфа": маркер всегда хочет уйти вправо, игрок тянет влево
                float drift = 1.0f;
                float playerControl = _isHoldingInput ? -2.5f : 0f;

                markerSlider.value += (drift + playerControl) * Time.deltaTime;
                markerSlider.value = Mathf.Clamp01(markerSlider.value);
            }

            // === Zone Check & Progress ===
            if (IsInSafeZone())
            {
                if (markerImage != null) markerImage.color = GameInterfaceManager.Instance.ColorReference.ConfirmGreen;
                HandleProgress(fillRate * Time.deltaTime);
            }
            else
            {
                if (markerImage != null) markerImage.color = GameInterfaceManager.Instance.ColorReference.ArcaCyan;
                HandleProgress(-drainRate * Time.deltaTime);
            }

            // === Timer ===
            if (timeExpires)
            {
                float remaining = Mathf.Max(0f, timeLimit - _currentTime);
                if (timerText != null)
                {
                    timerText.text = remaining.ToString("F1");
                    if (remaining <= 3f) timerText.color = GameInterfaceManager.Instance.ColorReference.AlertRed;
                }

                if (_currentTime >= timeLimit)
                {
                    ReportResult(InteractionState.Failure);
                }
            }
        }

        // Простая эмуляция прогресса взлома через локальную переменную или UI
        private float _hackProgress = 0f;

        void HandleProgress(float delta)
        {
            _hackProgress += delta;
            _hackProgress = Mathf.Clamp01(_hackProgress);
            progressSlider.value = _hackProgress;

            if (safeZoneRect != null)
            {
                float alpha = 0.5f + _hackProgress * 0.5f;
                Color c = safeZoneRect.GetComponent<Image>().color;
                c.a = alpha;
                safeZoneRect.GetComponent<Image>().color = c;
            }

            if (_hackProgress >= 1.0f)
            {
                ReportResult(InteractionState.Success);
            }
        }

        bool IsInSafeZone()
        {
            if (markerSlider == null || safeZoneRect == null) return false;

            float markerPos = markerSlider.value;
            float zoneMin = safeZoneRect.anchorMin.x;
            float zoneMax = safeZoneRect.anchorMax.x;

            return markerPos >= zoneMin && markerPos <= zoneMax;
        }

        void UpdateUI()
        {
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
        }
    }
}
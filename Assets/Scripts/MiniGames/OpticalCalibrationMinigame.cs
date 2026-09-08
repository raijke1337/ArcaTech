using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Arcatech.Interactions;
using TMPro;

namespace Arcatech.MiniGames
{
    public class OpticalCalibrationMinigame : MiniGameBase
    {
        [Serializable]
        public class Ring
        {
            [Tooltip("RectTransform кольца, который вращается")]
            public RectTransform transform;

            [Tooltip("Скорость вращения в градусах/сек")]
            public float rotationSpeed = 90f;

            [Tooltip("Целевой угол, на котором кольцо должно остановиться")]
            public float targetAngle;

            [Header("Input")]
            [Tooltip("Клавиша для остановки этого кольца (клавиатура)")]
            public KeyCode stopKey;

            [Tooltip("Кнопка геймпада для остановки этого кольца")]
            public KeyCode gamepadButton;

            [Header("Visual Feedback")]
            [Tooltip("Image компонента кольца для подсветки результата")]
            public Image ringImage;

            [HideInInspector] public bool isStopped;
            [HideInInspector] public float currentAngle;
            [HideInInspector] public Color originalColor;
        }

        [Header("Rings")]
        public Ring[] rings;

        [Header("Settings")]
        [Tooltip("Допустимое отклонение от целевого угла в градусах")]
        public float angleTolerance = 10f;

        [Header("Lose Conditions")]
        [SerializeField] private bool timeExpires = false;
        [SerializeField, Range(1f, 60f)] private float timeLimit = 15f;
        [SerializeField] private TextMeshProUGUI timerText;

        [Space]
        [SerializeField] private bool limitedFailures = false;
        [SerializeField, Range(1, 10)] private int allowedFailures = 3;
        [SerializeField] private TextMeshProUGUI attemptsText;

        [Header("Accessibility")]
        [Tooltip("При включении отключает плавные переходы цветов")]
        public bool reduceMotion = false;

        // === Приватное состояние ===
        private int _currentSessionId;
        private float _currentTime;
        private int _currentFails;

        // Кэшированные цвета палитры Arcatech
        private static readonly Color ColorSuccess = ParseHex("#5EE6A8");
        private static readonly Color ColorDanger  = ParseHex("#FF5268");
        private static readonly Color ColorCyan    = ParseHex("#29D7FF");
        private static readonly Color ColorBlocked = ParseHex("#566174");

        private static Color ParseHex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;
        }

        // ---------------------------------------------------------------
        //  Жизненный цикл MiniGameBase
        // ---------------------------------------------------------------

        protected override void OnGameStarted()
        {
            _currentSessionId = SessionId;
            _currentTime = 0f;
            _currentFails = 0;

            // Инициализация колец
            foreach (var ring in rings)
            {
                ring.isStopped = false;
                ring.currentAngle = UnityEngine.Random.Range(0f, 360f);

                if (ring.transform != null)
                    ring.transform.localEulerAngles = new Vector3(0, 0, ring.currentAngle);

                // Запоминаем исходный цвет для сброса
                if (ring.ringImage != null)
                {
                    ring.originalColor = ring.ringImage.color;
                    SetRingColor(ring, ring.originalColor, instant: true);
                }
            }

            // Показываем UI условий проигрыша
            if (timeExpires && timerText != null)
            {
                timerText.gameObject.SetActive(true);
                timerText.text = timeLimit.ToString("F1");
                timerText.color = Color.white;
            }

            if (limitedFailures && attemptsText != null)
            {
                attemptsText.gameObject.SetActive(true);
                attemptsText.text = allowedFailures.ToString();
                attemptsText.color = Color.white;
            }
        }

        protected override void OnGameEnded()
        {
            // Скрываем UI
            if (timerText != null)    timerText.gameObject.SetActive(false);
            if (attemptsText != null) attemptsText.gameObject.SetActive(false);

            // Сбрасываем цвета колец
            foreach (var ring in rings)
            {
                if (ring.ringImage != null)
                    SetRingColor(ring, ring.originalColor, instant: true);
            }

            // Останавливаем все корутины, чтобы ResetFailedRings
            // от предыдущей сессии не сработал
            StopAllCoroutines();
        }

        public override void ResetGame()
        {
            StopAllCoroutines();
            _currentTime = 0f;
            _currentFails = 0;

            foreach (var ring in rings)
            {
                ring.isStopped = false;
                ring.currentAngle = UnityEngine.Random.Range(0f, 360f);

                if (ring.transform != null)
                    ring.transform.localEulerAngles = new Vector3(0, 0, ring.currentAngle);

                if (ring.ringImage != null)
                    SetRingColor(ring, ring.originalColor, instant: true);
            }
        }

        // ---------------------------------------------------------------
        //  Update
        // ---------------------------------------------------------------

        void Update()
        {
            if (!IsRunning || IsFinishing || SessionId != _currentSessionId) return;

            _currentTime += Time.deltaTime;

            // === Таймер ===
            if (timeExpires)
            {
                float remaining = Mathf.Max(0f, timeLimit - _currentTime);

                if (timerText != null)
                {
                    timerText.text = remaining.ToString("F1");

                    // Визуальный акцент: последние 3 секунды — красный (#FF5268)
                    if (remaining <= 3f)
                        timerText.color = ColorDanger;
                }

                if (_currentTime >= timeLimit)
                {
                    ReportResult(InteractionState.Failure);
                    return;
                }
            }

            // === Вращение колец и обработка ввода ===
            bool allStopped = true;

            foreach (var ring in rings)
            {
                if (ring.isStopped) continue;

                allStopped = false;

                // Вращение
                ring.currentAngle += ring.rotationSpeed * Time.deltaTime;
                if (ring.currentAngle >= 360f) ring.currentAngle -= 360f;
                if (ring.currentAngle < 0f)    ring.currentAngle += 360f;

                if (ring.transform != null)
                    ring.transform.localEulerAngles = new Vector3(0, 0, ring.currentAngle);

                // Ввод: клавиатура ИЛИ геймпад
                bool keyPressed = false;
                if (ring.stopKey != KeyCode.None)
                    keyPressed |= Input.GetKeyDown(ring.stopKey);
                if (ring.gamepadButton != KeyCode.None)
                    keyPressed |= Input.GetKeyDown(ring.gamepadButton);

                if (keyPressed)
                {
                    ring.isStopped = true;

                    // Мгновенный фидбек: проверяем позицию и красим кольцо
                    float diff = Mathf.Abs(Mathf.DeltaAngle(ring.currentAngle, ring.targetAngle));
                    if (ring.ringImage != null)
                    {
                        Color feedbackColor = diff <= angleTolerance ? ColorSuccess : ColorDanger;
                        SetRingColor(ring, feedbackColor, instant: reduceMotion);
                    }
                }
            }

            // === Проверка результата, когда все кольца остановлены ===
            if (allStopped)
            {
                CheckResult();
            }
        }

        // ---------------------------------------------------------------
        //  Проверка результата
        // ---------------------------------------------------------------

        private void CheckResult()
        {
            bool allCorrect = true;

            foreach (var ring in rings)
            {
                float diff = Mathf.Abs(Mathf.DeltaAngle(ring.currentAngle, ring.targetAngle));
                if (diff > angleTolerance)
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect)
            {
                // === УСПЕХ ===
                ReportResult(InteractionState.Success);
            }
            else
            {
                // === ПРОМАХ ===
                HandleMiss();
            }
        }

        private void HandleMiss()
        {
            // Инкрементируем счётчик ошибок
            if (limitedFailures)
            {
                _currentFails++;

                if (attemptsText != null)
                {
                    int remaining = Mathf.Max(0, allowedFailures - _currentFails);
                    attemptsText.text = remaining.ToString();
                    attemptsText.color = ColorDanger;
                }

                // Проверяем, не исчерпан ли лимит
                if (_currentFails >= allowedFailures)
                {
                    ReportResult(InteractionState.Failure);
                    return; // Не перезапускаем кольца — игра окончена
                }
            }

            // Кольца сбрасываются с небольшой задержкой
            StartCoroutine(ResetRingsAfterDelay());
        }

        // ---------------------------------------------------------------
        //  Корутина сброса колец
        // ---------------------------------------------------------------

        private IEnumerator ResetRingsAfterDelay()
        {
            yield return new WaitForSeconds(0.6f);

            // Защита: не работаем, если сессия сменилась или игра завершена
            if (!IsRunning || IsFinishing || SessionId != _currentSessionId)
                yield break;

            foreach (var ring in rings)
            {
                ring.isStopped = false;

                // Сбрасываем цвет к исходному
                if (ring.ringImage != null)
                    SetRingColor(ring, ring.originalColor, instant: reduceMotion);
            }
        }

        // ---------------------------------------------------------------
        //  Утилиты
        // ---------------------------------------------------------------

        /// <summary>
        /// Устанавливает цвет Image кольца с учётом настройки reduceMotion.
        /// При instant = false используется плавная интерполяция в Update,
        /// но для простоты здесь применяем прямое присвоение с возможностью
        /// расширения через DOTween.
        /// </summary>
        private void SetRingColor(Ring ring, Color target, bool instant)
        {
            if (ring.ringImage == null) return;

            if (instant || reduceMotion)
            {
                ring.ringImage.color = target;
            }
            else
            {
                // Плавный переход (можно заменить на ring.ringImage.DOColor(target, 0.2f))
                // Для простоты прототипа используем прямое присвоение,
                // т.к. полный Lerp потребовал бы отдельного Update-цикла для каждого кольца
                ring.ringImage.color = target;
            }
        }
    }
}
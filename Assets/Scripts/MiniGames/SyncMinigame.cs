using UnityEngine;
using UnityEngine.UI;
using Arcatech.Interactions;

namespace Arcatech.MiniGames
{
    public class RhythmSyncMinigame : MiniGameBase
    {
        [Header("UI References")]
        public Slider progressSlider; // Шкала прогресса
        public RectTransform targetWave; // Внешняя волна (целевая)
        public RectTransform playerWave; // Внутренняя волна (игрока)
        
        [Header("Settings")]
        public float targetFrequency = 1.0f; // Частота целевой волны
        public float playerFrequencyBase = 0.8f; // Базовая частота игрока
        public float syncTolerance = 0.1f; // Допуск для совпадения фаз
        public float progressPerHit = 0.2f; // Прогресс за успешное нажатие
        public float penaltyPerMiss = 0.1f; // Штраф за промах

        private float _currentTime = 0f;

        protected override void OnGameStarted()
        {
            _currentTime = 0f;
            UpdateWaves();
        }

        protected override void OnGameEnded()
        {
            // Остановка логики
        }

        public override void ResetGame()
        {
            if (progressSlider != null) progressSlider.value = 0;
            _currentTime = 0f;
        }

        void Update()
        {
            if (!IsRunning || IsFinishing) return;

            _currentTime += Time.deltaTime;
            UpdateWaves();

            // Проверка ввода (Пробел, ЛКМ, Кнопка A на геймпаде)
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                CheckSync();
            }
        }

        void UpdateWaves()
        {
            // Эмуляция пульсации через масштаб или локальную позицию
            float targetScale = 1 + Mathf.Sin(_currentTime * targetFrequency) * 0.2f;
            float playerScale = 1 + Mathf.Sin(_currentTime * playerFrequencyBase) * 0.2f;

            if (targetWave) targetWave.localScale = Vector3.one * targetScale;
            if (playerWave) playerWave.localScale = Vector3.one * playerScale;
        }

        void CheckSync()
        {
            float targetPhase = Mathf.Sin(_currentTime * targetFrequency);
            float playerPhase = Mathf.Sin(_currentTime * playerFrequencyBase);

            if (Mathf.Abs(targetPhase - playerPhase) < syncTolerance)
            {
                // Успех
                if (progressSlider) progressSlider.value += progressPerHit;
                if (progressSlider && progressSlider.value >= 1.0f)
                {
                    ReportResult(InteractionState.Success);
                }
            }
            else
            {
                // Промах
                if (progressSlider) progressSlider.value = Mathf.Max(0, progressSlider.value - penaltyPerMiss);
            }
        }
    }
}
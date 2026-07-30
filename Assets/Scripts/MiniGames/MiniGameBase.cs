using System.Collections;
using Arcatech.Interactions;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.MiniGames
{
    public abstract class MiniGameBase : MonoBehaviour
    {
        [Header("Fade")] [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.2f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.2f;

        public UnityEvent<InteractionState> OnGameCompleteResult = new();

        private Coroutine _transitionRoutine;
        private int _sessionId;

        private bool _isRunning;
        private bool _isFinishing;

        protected bool IsRunning => _isRunning;
        protected bool IsFinishing => _isFinishing;

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            SetVisibleImmediately(false);
        }

        /// <summary>
        /// Запускает миниигру. Перед запуском всегда сбрасывает её состояние.
        /// </summary>
        public void StartGame()
        {
            _sessionId++;

            StopTransition();

            _isRunning = true;
            _isFinishing = false;

            gameObject.SetActive(true);

            // Важно: сбрасываем всё до fade-in.
            ResetGame();

            SetVisibleImmediately(false);

            OnGameStarted();

            _transitionRoutine = StartCoroutine(FadeInRoutine(_sessionId));
        }

        /// <summary>
        /// Завершает игру без отправки результата.
        /// Используется, например, при отмене interaction.
        /// </summary>
        public void EndGame()
        {
            if (!_isRunning && !gameObject.activeSelf)
            {
                return;
            }

            _sessionId++;

            StopTransition();

            _isRunning = false;
            _isFinishing = true;

            OnGameEnded();

            _transitionRoutine = StartCoroutine(FadeOutRoutine(_sessionId, null));
        }

        /// <summary>
        /// Этот метод должен сбрасывать игровое состояние:
        /// таймеры, очки, выбранные кнопки, позиции объектов и т.д.
        /// </summary>
        public abstract void ResetGame();

        /// <summary>
        /// Вызывается после ResetGame и до fade-in.
        /// Здесь можно запускать внутреннюю игровую логику.
        /// </summary>
        protected virtual void OnGameStarted()
        {
        }

        /// <summary>
        /// Вызывается при штатном окончании или отмене игры.
        /// Здесь нужно останавливать корутины, звук и прочую игровую логику.
        /// </summary>
        protected virtual void OnGameEnded()
        {
        }

        /// <summary>
        /// Вызывать из конкретной миниигры при победе, проигрыше или ином результате.
        /// Событие будет отправлено только после fade-out.
        /// </summary>
        protected void ReportResult(InteractionState result)
        {
            if (!_isRunning || _isFinishing)
            {
                return;
            }

            _sessionId++;

            StopTransition();

            _isRunning = false;
            _isFinishing = true;

            OnGameEnded();

            _transitionRoutine = StartCoroutine(FadeOutRoutine(_sessionId, result));
        }

        private IEnumerator FadeInRoutine(int sessionId)
        {
            yield return FadeTo(1f, fadeInDuration);

            if (sessionId != _sessionId)
            {
                yield break;
            }

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            _transitionRoutine = null;
        }

        private IEnumerator FadeOutRoutine(int sessionId, InteractionState? result)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            yield return FadeTo(0f, fadeOutDuration);

            if (sessionId != _sessionId)
            {
                yield break;
            }

            _isFinishing = false;
            _transitionRoutine = null;

            gameObject.SetActive(false);

            if (result.HasValue)
            {
                OnGameCompleteResult?.Invoke(result.Value);
            }
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float startAlpha = canvasGroup.alpha;

            if (duration <= 0f)
            {
                canvasGroup.alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                canvasGroup.alpha = Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    elapsed / duration);

                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        private void StopTransition()
        {
            if (_transitionRoutine == null)
            {
                return;
            }

            StopCoroutine(_transitionRoutine);
            _transitionRoutine = null;
        }

        private void SetVisibleImmediately(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
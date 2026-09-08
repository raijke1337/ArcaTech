using System.Collections;
using Arcatech.Interactions;
using Arcatech.Texts;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.MiniGames
{
    public abstract class MiniGameBase : MonoBehaviour
    {
        public UnityEvent<InteractionState> onGameCompleteResult;

        private Coroutine _transitionRoutine;
        private int _sessionId;

        protected int SessionId => _sessionId;
        private bool _isRunning;
        private bool _isFinishing;

        protected bool IsRunning => _isRunning;
        protected bool IsFinishing => _isFinishing;
        [SerializeField] Description description;
        public Description Description => description;
        
        /// <summary>
        /// Запускает миниигру. Перед запуском всегда сбрасывает её состояние.
        /// </summary>
        public void StartGame()
        {
            _sessionId++;


            _isRunning = true;
            _isFinishing = false;
            ResetGame();
            OnGameStarted();
        }

        /// <summary>
        /// Завершает игру без отправки результата.
        /// Используется, например, при отмене interaction.
        /// </summary>
        public void CancelGame()
        {
            if (!_isRunning && !gameObject.activeSelf)
            {
                return;
            }

            _sessionId++;
            _isRunning = false;
            _isFinishing = true;

            OnGameEnded();

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
        protected abstract void OnGameStarted();

        /// <summary>
        /// Вызывается при штатном окончании или отмене игры.
        /// Здесь нужно останавливать корутины, звук и прочую игровую логику.
        /// </summary>
        protected abstract void OnGameEnded();

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
            _isRunning = false;
            _isFinishing = true;
            OnGameEnded();
            onGameCompleteResult?.Invoke(result);
        }
    }
}
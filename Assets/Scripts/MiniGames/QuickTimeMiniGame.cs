using System.Collections;
using DG.Tweening;
using Arcatech.Cameras;
using Arcatech.Interactions;
using Arcatech.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arcatech.MiniGames
{
    public class QuickTimeMiniGame : MiniGameBase
    {

        [SerializeField] private Image TargetRing;
        [SerializeField] private Image TargeterRing;
        [SerializeField] private Image ButtonImage;

        [Space, Header("Game Settings")]

        [SerializeField] private MiniGameButton buttonToPress = MiniGameButton.ButtonS;

        [SerializeField] private float targeterStartScale = 1.5f;
        [SerializeField] private float targeterSuccessTarget = 0.88f;
        [SerializeField] private float targeterSuccessGrace = 0.18f;
        [SerializeField] private float targetTime = 1.5f;

        [Space, Header("Juice")]
        [SerializeField] private float targeterFadeInDuration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float targetPulseMinAlpha = 0.4f;
        [SerializeField] private float targetPulseDuration = 0.6f;
        [SerializeField] private float resultPunchScale = 0.25f;
        [SerializeField] private float resultPunchDuration = 0.3f;
        [SerializeField, Tooltip("Сколько реального времени панель держит цвет результата, прежде чем закрыться. Должно быть не меньше resultPunchDuration.")]
        private float resultDisplayDuration = 0.5f;

        [Space, Header("Slow-mo & Camera")]
        [SerializeField, Range(0.05f, 1f)] private float hitstopSpeedFraction = 0.35f;
        [SerializeField] private float zoomFieldOfView = 25f;
        [SerializeField] private float zoomInDuration = 0.2f;
        [SerializeField] private float zoomOutDuration = 0.35f;

        // === Private State ===
        private int _currentSessionId;
        private float _elapsedTime;
        private float _shrinkSpeed;
        private bool _isWaitingForInput;

        private Tween _targeterFadeTween;
        private Tween _targetPulseTween;
        private Tween _resultPunchTween;
        private Coroutine _resultRoutine;

        public override void ResetGame()
        {
            _elapsedTime = 0f;
            _isWaitingForInput = true;

            if (_resultRoutine != null)
            {
                StopCoroutine(_resultRoutine);
                _resultRoutine = null;
            }

            KillTweens(complete: true);

            float lowerBound = targeterSuccessTarget - targeterSuccessGrace;
            float shrinkDistance = targeterStartScale - lowerBound;

            _shrinkSpeed = targetTime > 0f
                ? shrinkDistance / targetTime
                : shrinkDistance;

            SetRingScale(TargeterRing, targeterStartScale);
            SetRingScale(TargetRing, targeterSuccessTarget);
            SetAlpha(TargeterRing, 0f);
        }

        protected override void OnGameStarted()
        {
            _currentSessionId = SessionId;

            TargeterRing.color = SetColorAlpha(GameInterfaceManager.Instance.ColorReference.ArcaCyan, 0f);
            TargetRing.color = SetColorAlpha(GameInterfaceManager.Instance.ColorReference.TechnoBlue, 1f);

            // NYI
            // ButtonImage = GameInterfaceManager.Instance.GetButtonImage(buttonToPress);

            _targeterFadeTween = TargeterRing
                .DOFade(1f, targeterFadeInDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

            _targetPulseTween = TargetRing
                .DOFade(targetPulseMinAlpha, targetPulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

            float hitstopRealtime = targetTime / Mathf.Max(0.0001f, hitstopSpeedFraction);
            HitstopPlayer.Instance.PlayHitstop(hitstopRealtime, hitstopSpeedFraction);

            CamerasController.Instance.ZoomTo(zoomFieldOfView, zoomInDuration);
        }

        protected override void OnGameEnded()
        {
            _isWaitingForInput = false;

            _targeterFadeTween?.Kill();
            _targetPulseTween?.Kill();

            HitstopPlayer.Instance.StopHitstop();
            CamerasController.Instance.ResetZoom(zoomOutDuration);
        }

        private void Update()
        {
            if (!IsRunning || IsFinishing || SessionId != _currentSessionId) return;
            if (!_isWaitingForInput) return;

            _elapsedTime += Time.deltaTime;

            float currentScale = Mathf.Max(0f, targeterStartScale - _shrinkSpeed * _elapsedTime);
            SetRingScale(TargeterRing, currentScale);

            if (currentScale <= targeterSuccessTarget - targeterSuccessGrace)
            {
                _isWaitingForInput = false;
                PlayResultFeedback(false);
                _resultRoutine = StartCoroutine(DelayedReportResult(InteractionState.Failure, _currentSessionId));
            }
        }

        protected override void HandleButtonPress(MiniGameButton button, InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (!_isWaitingForInput) return;

            _isWaitingForInput = false;

            if (button != buttonToPress)
            {
                PlayResultFeedback(false);
                _resultRoutine = StartCoroutine(DelayedReportResult(InteractionState.Failure, _currentSessionId));
                return;
            }

            float currentScale = targeterStartScale - _shrinkSpeed * _elapsedTime;
            bool isMatch = Mathf.Abs(currentScale - targeterSuccessTarget) <= targeterSuccessGrace;

            PlayResultFeedback(isMatch);
            _resultRoutine = StartCoroutine(
                DelayedReportResult(isMatch ? InteractionState.Success : InteractionState.Failure, _currentSessionId));
        }

        /// <summary>
        /// Даёт игроку время увидеть цвет/punch результата, прежде чем сообщить
        /// о завершении наружу (что запускает закрытие панели).
        /// Ждём в реальном времени, т.к. в момент вызова hitstop ещё может
        /// замедлять Time.timeScale.
        /// </summary>
        private IEnumerator DelayedReportResult(InteractionState state, int session)
        {
            yield return new WaitForSecondsRealtime(resultDisplayDuration);

            _resultRoutine = null;

            // Защита от случая, если за время ожидания игру успели перезапустить
            // (новый StartGame() поднял SessionId) — не репортим устаревший результат.
            if (session != SessionId) yield break;

            ReportResult(state);
        }

        private void PlayResultFeedback(bool success)
        {
            var color = success
                ? GameInterfaceManager.Instance.ColorReference.ConfirmGreen
                : GameInterfaceManager.Instance.ColorReference.AlertRed;

            TargeterRing.DOColor(color, 0.1f);
            TargetRing.DOColor(color, 0.1f);
            
            _resultPunchTween?.Kill();
            _resultPunchTween = TargeterRing.rectTransform
                .DOPunchScale(Vector3.one * resultPunchScale, resultPunchDuration, vibrato: success ? 4 : 8)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void KillTweens(bool complete)
        {
            _targeterFadeTween?.Kill(complete);
            _targetPulseTween?.Kill(complete);
            _resultPunchTween?.Kill(complete);
        }

        private static void SetRingScale(Image ring, float scale)
        {
            if (ring == null) return;
            ring.rectTransform.localScale = Vector3.one * scale;
        }

        private static void SetAlpha(Image image, float alpha)
        {
            if (image == null) return;
            image.color = SetColorAlpha(image.color, alpha);
        }

        private static Color SetColorAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
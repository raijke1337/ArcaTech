using System.Collections;
using System.Collections.Generic;
using Arcatech.Interactions;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units.Control;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arcatech.MiniGames
{
    [RequireComponent(typeof(PanelAnimator))]
    public abstract class MiniGameBase : ValidatedMonoBehaviour,PlayerControls.IMinigameActions
    {
        [SerializeField,Self]public PanelAnimator Animator;
        public bool ShowInGadget = true;
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

        [Header("Button glyphs")]
        [Tooltip("Иконки кнопок. Пусто - в Image остаются спрайты, заданные в префабе.")]
        [SerializeField] private InputGlyphSet glyphs;

        private readonly List<(Image image, MiniGameButton button)> _glyphImages = new();
        private PlayerInputGateway _glyphGateway;

        /// <summary>
        /// Привязывает Image к кнопке миниигры: иконка берётся под текущее устройство
        /// и сама меняется при его смене. Вызывать из OnGameStarted.
        /// </summary>
        protected void BindGlyph(Image image, MiniGameButton button)
        {
            if (image == null) return;
            _glyphImages.Add((image, button));
            ApplyGlyph(image, button);
        }

        private void ApplyGlyph(Image image, MiniGameButton button)
        {
            PlayerInputGateway gateway = PlayerInputGateway.Instance;
            if (gateway == null || image == null) return;

            if (InputPromptFormatter.TryGetActionSprite($"Minigame/{button}", gateway.Actions,
                    gateway.ActiveDeviceKind, gateway.LastUsedDevice, glyphs, out Sprite sprite))
            {
                image.sprite = sprite;
            }
        }

        private void RefreshGlyphs(InputDeviceKind kind, InputDevice device)
        {
            foreach (var (image, button) in _glyphImages) ApplyGlyph(image, button);
        }

        private void SubscribeGlyphs()
        {
            UnsubscribeGlyphs();
            _glyphImages.Clear();
            _glyphGateway = PlayerInputGateway.Instance;
            if (_glyphGateway != null) _glyphGateway.ActiveDeviceChanged += RefreshGlyphs;
        }

        private void UnsubscribeGlyphs()
        {
            if (_glyphGateway != null) _glyphGateway.ActiveDeviceChanged -= RefreshGlyphs;
            _glyphGateway = null;
        }

        private void OnDestroy() => UnsubscribeGlyphs();
        
        /// <summary>
        /// Запускает миниигру. Перед запуском всегда сбрасывает её состояние.
        /// </summary>
        public void StartGame()
        {
            _sessionId++;
            _isRunning = true;
            _isFinishing = false;
            ResetGame();
            SubscribeGlyphs();
            PlayerInputGateway.Instance?.EnableMinigameInput(this);
            OnGameStarted();
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
            UnsubscribeGlyphs();
            onGameCompleteResult?.Invoke(result);
            PlayerInputGateway.Instance?.EnableGameplayInput();
        }

        protected abstract void HandleButtonPress(MiniGameButton button, InputAction.CallbackContext context);

        public void OnButtonN(InputAction.CallbackContext context) => Press(MiniGameButton.ButtonN, context);
        public void OnButtonE(InputAction.CallbackContext context) => Press(MiniGameButton.ButtonE, context);
        public void OnButtonW(InputAction.CallbackContext context) => Press(MiniGameButton.ButtonW, context);
        public void OnButtonS(InputAction.CallbackContext context) => Press(MiniGameButton.ButtonS, context);

        private void Press(MiniGameButton button, InputAction.CallbackContext context)
        {
            PlayerInputGateway.Instance?.NotifyDeviceUsed(context);
            HandleButtonPress(button, context);
        }
    }
    
}
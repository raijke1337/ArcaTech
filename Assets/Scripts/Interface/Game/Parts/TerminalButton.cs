using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    [RequireComponent(typeof(Image))]
    public class TerminalButton : Button
    {

        [Header("Visual Elements")] [SerializeField]
        private Image _background; // BG (text)

        [SerializeField] private Image _frame; // Frame

        [Header("Background Colors")] [SerializeField]
        private Color _bgNormal = new Color(0.2f, 0.2f, 0.2f);

        [SerializeField] private Color _bgHover = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _bgPressed = new Color(0.1f, 0.1f, 0.1f);
        [SerializeField] private Color _bgSelected = new Color(0.25f, 0.25f, 0.35f);
        [SerializeField] private Color _bgDisabled = new Color(0.15f, 0.15f, 0.15f);

        [Header("Frame Colors")] [SerializeField]
        private Color _frameNormal = new Color(0.5f, 0.5f, 0.5f);

        [SerializeField] private Color _frameHover = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color _framePressed = Color.white;
        [SerializeField] private Color _frameSelected = new Color(0.7f, 0.7f, 1f);
        [SerializeField] private Color _frameDisabled = new Color(0.3f, 0.3f, 0.3f);

        [Header("Animation")] [SerializeField] private float _fadeDuration = 0.15f;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            // Сначала пускаем стандартную логику Button 
            // (она ничего не сделает, т.к. transition = None, но это безопасно)
            base.DoStateTransition(state, instant);

            Color bgColor = _bgNormal;
            Color frameColor = _frameNormal;

            switch (state)
            {
                case SelectionState.Normal:
                    bgColor = _bgNormal;
                    frameColor = _frameNormal;
                    break;
                case SelectionState.Highlighted:
                    bgColor = _bgHover;
                    frameColor = _frameHover;
                    break;
                case SelectionState.Pressed:
                    bgColor = _bgPressed;
                    frameColor = _framePressed;
                    break;
                case SelectionState.Selected:
                    bgColor = _bgSelected;
                    frameColor = _frameSelected;
                    break;
                case SelectionState.Disabled:
                    bgColor = _bgDisabled;
                    frameColor = _frameDisabled;
                    break;
            }

            ApplyColors(bgColor, frameColor, instant);
        }

        private void ApplyColors(Color bgColor, Color frameColor, bool instant)
        {
            float duration = instant ? 0f : _fadeDuration;

            if (_background != null)
                _background.CrossFadeColor(bgColor, duration, true, true);

            if (_frame != null)
                _frame.CrossFadeColor(frameColor, duration, true, true);
        }

#if UNITY_EDITOR
        // Автоматически выставляем Transition = None при добавлении компонента
        protected override void Reset()
        {
            base.Reset();
            transition = Transition.None;
        }
#endif

    }
}
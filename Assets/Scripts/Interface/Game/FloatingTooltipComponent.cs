using Arcatech.Interactions;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arcatech.UI
{
    /// <summary>
    /// Вью плавающей подсказки. Ничего не решает сама: что и когда показывать,
    /// определяет TargetTooltipController.
    /// Мышь - подсказка следует за курсором; геймпад - висит над объектом.
    /// </summary>
    [RequireComponent(typeof(PanelAnimator))]
    public class FloatingTooltipComponent : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private PanelAnimator animator;
        [SerializeField] private TextMeshProUGUI TitleLabel;
        [SerializeField] private TextMeshProUGUI InteractiveLabel;
        [SerializeField, Self] RectTransform rectT;

        [Header("Позиция")]
        [Tooltip("Смещение от курсора, чтобы подсказка его не перекрывала (px).")]
        [SerializeField] private Vector2 pointerOffset = new(30f, -30f);
        [Tooltip("Смещение от точки объекта на экране в режиме геймпада (px).")]
        [SerializeField] private Vector2 worldAnchorOffset = new(0f, 60f);

        private ITargetTooltip _anchorTarget;
        private bool _followWorld;
        private Camera _camera;
        private readonly Vector3[] _corners = new Vector3[4];

        public PanelAnimator PanelAnimator => animator;

        private void Awake()
        {
            // Подсказка не должна ловить рейкасты UI - иначе она "перекроет" сама себя
            // и TargetTooltipController решит, что курсор над интерфейсом.
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        #region API для TargetTooltipController

        /// <summary>Старый вызов: заголовок и текст как есть, без подстановки кнопок.</summary>
        public void Set(ITargetTooltip tgt)
        {
            SetContent(tgt?.TooltipTitle, tgt?.TooltipBody);
        }

        public void SetContent(string title, string body)
        {
            SetLabel(TitleLabel, title);
            SetLabel(InteractiveLabel, body);
        }

        public void SetSpriteAsset(TMP_SpriteAsset spriteAsset)
        {
            if (spriteAsset == null) return;
            if (InteractiveLabel != null) InteractiveLabel.spriteAsset = spriteAsset;
            if (TitleLabel != null) TitleLabel.spriteAsset = spriteAsset;
        }

        public void FollowPointer()
        {
            _followWorld = false;
            _anchorTarget = null;
            UpdatePosition();
        }

        public void FollowWorld(ITargetTooltip target)
        {
            _followWorld = true;
            _anchorTarget = target;
            UpdatePosition();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UpdatePosition();
            animator.Show();
        }

        public void Hide()
        {
            _anchorTarget = null;
            if (gameObject.activeInHierarchy) animator.Hide();
        }

        #endregion

        private static void SetLabel(TextMeshProUGUI label, string text)
        {
            if (label == null) return;
            bool hasText = !string.IsNullOrEmpty(text);
            label.text = hasText ? text : string.Empty;
            label.gameObject.SetActive(hasText);
        }

        private void UpdatePosition()
        {
            if (!TryGetScreenPoint(out Vector2 screenPoint)) return;
            rectT.position = screenPoint;
            ClampToScreen();
        }

        private bool TryGetScreenPoint(out Vector2 screenPoint)
        {
            screenPoint = default;

            if (_followWorld)
            {
                // Цель могла быть уничтожена между кадрами - Unity-null.
                if (_anchorTarget == null || (_anchorTarget is Object o && o == null)) return false;

                if (_camera == null) _camera = Camera.main;
                if (_camera == null) return false;

                Vector3 sp = _camera.WorldToScreenPoint(_anchorTarget.TooltipAnchor);
                if (sp.z <= 0f) return false; // за камерой

                screenPoint = (Vector2)sp + worldAnchorOffset;
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null) return false;

            screenPoint = mouse.position.ReadValue() + pointerOffset;
            return true;
        }

        private void ClampToScreen()
        {
            rectT.GetWorldCorners(_corners);
            Vector3 position = rectT.position;

            if (_corners[2].x > Screen.width) position.x -= _corners[2].x - Screen.width;
            if (_corners[0].x < 0) position.x -= _corners[0].x;
            if (_corners[2].y > Screen.height) position.y -= _corners[2].y - Screen.height;
            if (_corners[0].y < 0) position.y -= _corners[0].y;

            rectT.position = position;
        }
    }
}

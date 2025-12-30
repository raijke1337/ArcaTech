using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Arcatech.UI
{

    public class DamageTextUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform rect;
        private Sequence seq;

        private void Awake()
        {
            rect = transform as RectTransform;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        }

        // Plays a pop + float up + fade animation
        public void Play(string content, Color color, Vector2 startPos, float upwardDistance, float duration, System.Action onComplete)
        {
            // Kill any in-flight animation
            seq?.Kill(false);

            // Initialize visuals
            rect.anchoredPosition = startPos;
            rect.localScale = Vector3.one * 0.85f;
            canvasGroup.alpha = 1f;

            text.text = content;
            text.color = color;

            // Build animation
            seq = DOTween.Sequence();

            // Start movement immediately
            seq.Join(rect.DOAnchorPosY(startPos.y + upwardDistance, duration).SetEase(Ease.OutCubic));

            // Pop scale in the beginning
            seq.Join(rect.DOScale(1f, 0.22f).SetEase(Ease.OutBack));

            // Fade out mostly over the latter portion
            float fadeDelay = Mathf.Clamp(duration * 0.2f, 0f, duration);
            float fadeTime = Mathf.Max(0.18f, duration - fadeDelay);
            seq.Join(canvasGroup.DOFade(0f, fadeTime).SetDelay(fadeDelay).SetEase(Ease.InQuad));

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void StopAndReset()
        {
            seq?.Kill(false);
            canvasGroup.alpha = 0f;
            rect.localScale = Vector3.one;
        }

        private void OnDisable()
        {
            // Ensure tweens are killed to avoid leaking
            seq?.Kill(false);
        }
    }
}
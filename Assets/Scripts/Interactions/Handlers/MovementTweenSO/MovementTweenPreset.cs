using DG.Tweening;
using UnityEngine;
namespace Arcatech
{
    [CreateAssetMenu(fileName = "MovementTweenPreset", menuName = "Tweening/Movement Preset")]
    public class MovementTweenPreset : SerializedDOTweener
    {
        [Header("Movement Settings")]
        public Vector3 targetPosition;
        public bool useLocalPosition = true;
        public bool isRelative = false;  // If true, adds to current position
        public bool snapping = false;    // Snap to integer values

        [Header("Timing")]
        public float duration = 1f;
        public float delay = 0f;

        [Header("Easing")]
        public bool useCustomCurve = false;
        public AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public Ease easeType = Ease.OutQuad;

        [Header("Loop Settings")]
        public int loops = 0;  // 0 = no loop, -1 = infinite
        public LoopType loopType = LoopType.Restart;

        [Header("Callbacks")]
        public bool useOnComplete = false;
        public UnityEngine.Events.UnityEvent onCompleteEvent;

        protected override Tween Build(Transform target)
        {
            Tween tween;

            if (useLocalPosition)
            {
                tween = target.DOLocalMove(targetPosition, duration, snapping);
            }
            else
            {
                tween = target.DOMove(targetPosition, duration, snapping);
            }

            // Apply settings
            tween.SetRelative(isRelative)
                 .SetDelay(delay)
                 .SetLoops(loops, loopType);

            // Set easing
            if (useCustomCurve)
            {
                tween.SetEase(customCurve);
            }
            else
            {
                tween.SetEase(easeType);
            }

            // Add callbacks if needed
            if (useOnComplete && onCompleteEvent != null)
            {
                tween.OnComplete(() => onCompleteEvent.Invoke());
            }

            return tween;
        }
    }
}

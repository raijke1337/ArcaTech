using DG.Tweening;
using UnityEngine;

namespace Arcatech
{
    [CreateAssetMenu(fileName = "tweenSO_movement_", menuName = "Tweening/Movement Preset")]
    public class MovementTweenPreset : SerializedDOTweener
    {
        [Header("Movement Settings")] public Vector3 targetPosition;
        public bool isRelative = false;
        public bool snapping = false;

        [Header("Timing")] public float duration = 1f;
        public float delay = 0f;

        [Header("Easing")] public bool useCustomCurve = false;
        public AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public Ease easeType = Ease.InOutSine; // InOutSine лучше для плавного движения

        [Header("Loop Settings")] public int loops = -1; // -1 = бесконечно
        public LoopType loopType = LoopType.Yoyo;

        [Header("Callbacks")] public bool useOnComplete = false;
        public UnityEngine.Events.UnityEvent onCompleteEvent;

        protected override Tween Build(Transform target)
        {
            Tween tween;

            if (isRelative)
            {
                if (target.TryGetComponent(out Rigidbody rb))
                {
                    // Rigidbody tweens use world coordinates, so rotate the local
                    // offset by the parent before applying it as a world-space delta.
                    Vector3 worldOffset = target.parent
                        ? target.parent.TransformVector(targetPosition)
                        : targetPosition;
                    tween = rb.DOMove(worldOffset, duration, snapping).SetRelative();
                }
                else
                {
                    // Relative movement is defined in the target's parent space.
                    tween = target.DOLocalMove(targetPosition, duration, snapping).SetRelative();
                }
            }
            else
            {
                if (target.TryGetComponent(out Rigidbody rb))
                {
                    tween = rb.DOMove(targetPosition, duration, snapping);
                }
                else
                {
                    tween = target.DOMove(targetPosition, duration, snapping);
                }
            }

            tween.SetDelay(delay)
                .SetLoops(loops, loopType)
                .SetAutoKill(false) // НЕ убивать автоматически
                .SetUpdate(UpdateType.Normal, true); // Работать даже если Time.timeScale = 0

            if (useCustomCurve)
            {
                tween.SetEase(customCurve);
            }
            else
            {
                tween.SetEase(easeType);
            }

            if (useOnComplete && onCompleteEvent != null)
            {
                tween.OnComplete(() => onCompleteEvent.Invoke());
            }

            return tween;
        }
    }
}

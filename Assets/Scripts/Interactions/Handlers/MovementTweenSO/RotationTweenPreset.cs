using DG.Tweening;
using UnityEngine;
namespace Arcatech
{
    [CreateAssetMenu(fileName = "tweenSO_rotation_", menuName = "Tweening/Rotation Preset")]
    public class RotationTweenPreset : SerializedDOTweener
    {
        public enum RotationMode
        {
            Absolute,       // Rotate to exact angles
            Relative,       // Add to current rotation
            LookAt          // Look at position
        }

        [Header("Rotation Settings")]
        public RotationMode rotationMode = RotationMode.Absolute;
        public Vector3 targetRotation;
        public Vector3 lookAtTarget;  // Used only if RotationMode is LookAt
        public bool useLocalRotation = true;

        [Header("Rotation Specifics")]
        public RotateMode rotateMode = RotateMode.FastBeyond360;  // How to handle >360° rotations

        [Header("Timing")]
        public float duration = 1f;
        public float delay = 0f;

        [Header("Easing")]
        public bool useCustomCurve = false;
        public AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public Ease easeType = Ease.OutQuad;

        [Header("Loop Settings")]
        public int loops = 0;
        public LoopType loopType = LoopType.Restart;

        [Header("Callbacks")]
        public bool useOnComplete = false;
        public UnityEngine.Events.UnityEvent onCompleteEvent;

        protected override Tween Build(Transform target)
        {
            Tween tween = null;

            switch (rotationMode)
            {
                case RotationMode.Absolute:
                    if (useLocalRotation)
                    {
                        tween = target.DOLocalRotate(targetRotation, duration, rotateMode);
                    }
                    else
                    {
                        tween = target.DORotate(targetRotation, duration, rotateMode);
                    }
                    break;

                case RotationMode.Relative:
                    if (useLocalRotation)
                    {
                        tween = target.DOLocalRotate(targetRotation, duration, rotateMode)
                                      .SetRelative(true);
                    }
                    else
                    {
                        tween = target.DORotate(targetRotation, duration, rotateMode)
                                      .SetRelative(true);
                    }
                    break;

                case RotationMode.LookAt:
                    tween = target.DOLookAt(lookAtTarget, duration);
                    break;
            }

            if (tween != null)
            {
                // Apply settings
                tween.SetDelay(delay)
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

                // Add callbacks
                if (useOnComplete && onCompleteEvent != null)
                {
                    tween.OnComplete(() => onCompleteEvent.Invoke());
                }
            }

            return tween;
        }
    }

}
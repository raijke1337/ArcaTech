namespace Arcatech.Interactions
{
    using UnityEngine;
    using DG.Tweening;

    [CreateAssetMenu(fileName = "ShakeTweenPreset", menuName = "Tweening/Shaking Preset")]
    public class ShakeTweenPreset : SerializedDOTweener
    {
        [Header("Basic Settings")] [SerializeField]
        private float duration = 1f;

        [Header("Strength")] [SerializeField] private Vector3 strengthVector = Vector3.one;

        [Header("Shake Properties")] [SerializeField, Range(1, 50)]
        private int vibrato = 10;

        [SerializeField, Range(0f, 180f)] private float randomness = 90f;
        [SerializeField] private bool snapping = false;
        [SerializeField] private bool fadeOut = true;
        [SerializeField] private ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full;

        [Header("Tween Settings")] [SerializeField]
        private Ease easeType = Ease.Linear;

        [SerializeField] private bool setRelative = false;
        [SerializeField] private int loops = 1;
        [SerializeField] private LoopType loopType = LoopType.Restart;
        [SerializeField] private float delay = 0f;

        #region Properties

        public float Duration
        {
            get => duration;
            set => duration = Mathf.Max(0f, value);
        }


        public Vector3 StrengthVector
        {
            get => strengthVector;
            set => strengthVector = value;
        }

        public int Vibrato
        {
            get => vibrato;
            set => vibrato = Mathf.Clamp(value, 1, 50);
        }

        public float Randomness
        {
            get => randomness;
            set => randomness = Mathf.Clamp(value, 0f, 180f);
        }

        public bool Snapping
        {
            get => snapping;
            set => snapping = value;
        }

        public bool FadeOut
        {
            get => fadeOut;
            set => fadeOut = value;
        }

        public ShakeRandomnessMode RandomnessMode
        {
            get => randomnessMode;
            set => randomnessMode = value;
        }

        public Ease EaseType
        {
            get => easeType;
            set => easeType = value;
        }

        public bool SetRelative
        {
            get => setRelative;
            set => setRelative = value;
        }

        public int Loops
        {
            get => loops;
            set => loops = value;
        }

        public LoopType LoopType
        {
            get => loopType;
            set => loopType = value;
        }

        public float Delay
        {
            get => delay;
            set => delay = Mathf.Max(0f, value);
        }

        #endregion

        /// <summary>
        /// Builds a paused DOShakePosition tween with the configured settings
        /// </summary>
        /// <param name="target">The Transform to shake</param>
        /// <returns>A paused Tween ready to be played</returns>
        protected override Tween Build(Transform target)
        {
            if (target == null)
            {
                Debug.LogError("Target Transform is null!");
                return null;
            }

            Tween shakeTween = target.DOShakePosition(
                duration,
                strengthVector,
                vibrato,
                randomness,
                snapping,
                fadeOut,
                randomnessMode
            ).Pause();

            // Apply additional tween settings
            shakeTween.SetEase(easeType)
                .SetRelative(setRelative)
                .SetLoops(loops, loopType)
                .SetDelay(delay);

            return shakeTween;
        }

        /// <summary>
        /// Validates the configuration values
        /// </summary>
        private void OnValidate()
        {
            duration = Mathf.Max(0f, duration);
            vibrato = Mathf.Clamp(vibrato, 1, 50);
            randomness = Mathf.Clamp(randomness, 0f, 180f);
            delay = Mathf.Max(0f, delay);
        }
    }
}
using Arcatech.Managers;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;

namespace ArcaTech.UI
{
    public class GlitchController : GenericLazySingleton<GlitchController>
    {
        [Header("Material from Render Feature")] [SerializeField]
        private Material glitchMaterial;

        [Header("Glitch Visual")] [SerializeField]
        private float maxIntensity = 1f;

        [SerializeField] private float rgbSplit = 0.01f;
        [SerializeField] private float blockSize = 30f;
        [SerializeField] private float blockAmount = 0.5f;
        [SerializeField] private float scanline = 0.3f;
        [SerializeField] private float noise = 0.2f;
        [SerializeField] private float speed = 10f;
        [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float duration = 0.4f;

        [Header("Camera Shake (Cinemachine Impulse)")]
        [Tooltip("Источник импульса на этом объекте. Сигнал/гейны/радиус настраиваются на самом компоненте в инспекторе.")]
        [SerializeField,Self]
        private CinemachineImpulseSource impulseSource;

        [Tooltip("Сила тряски (множитель к velocityScale).")]
        [SerializeField]
        private float shakeAmplitude = 1f;

        [Tooltip("Длительность затухания тряски (сек).")] [SerializeField]
        private float shakeDuration = 0.4f;

        [Tooltip("Кривая огибающей (1 = старт, 0 = конец).")]
        [SerializeField]
        private AnimationCurve shakeEnvelope = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(1f, 0f, -2f, -2f));

        [Tooltip("Менять направление каждый кадр (хаотично) или держать стабильным.")] [SerializeField]
        private bool randomizeDirection = true;

        // Свойства шейдера
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int RGBSplitId = Shader.PropertyToID("_RGBSplit");
        private static readonly int BlockSizeId = Shader.PropertyToID("_BlockSize");
        private static readonly int BlockAmountId = Shader.PropertyToID("_BlockAmount");
        private static readonly int ScanlineId = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int NoiseId = Shader.PropertyToID("_NoiseIntensity");
        private static readonly int SpeedId = Shader.PropertyToID("_Speed");

        private float _timer;
        private bool _isGlitching;
        private float _currentMaxIntensity;

        private float _shakeTimer;
        private bool _isShaking;
        private float _shakeDurationActive;
        private Vector3 _shakeDirection;

        void Awake()
        {
            // --- Материал ---
            if (glitchMaterial != null)
            {
                glitchMaterial.SetFloat(IntensityId, 0f);
                glitchMaterial.SetFloat(RGBSplitId, rgbSplit);
                glitchMaterial.SetFloat(BlockSizeId, blockSize);
                glitchMaterial.SetFloat(BlockAmountId, blockAmount);
                glitchMaterial.SetFloat(ScanlineId, scanline);
                glitchMaterial.SetFloat(NoiseId, noise);
                glitchMaterial.SetFloat(SpeedId, speed);
            }

            // --- Источник тряски: берём прицепленный компонент ---
            if (impulseSource == null)
                impulseSource = GetComponent<CinemachineImpulseSource>();

            if (impulseSource == null)
                Debug.LogWarning("[GlitchController] CinemachineImpulseSource не найден — тряска работать не будет.", this);
        }

        void Update()
        {
            // --- Визуальный глитч ---
            if (_isGlitching && glitchMaterial != null)
            {
                _timer += Time.deltaTime;
                float t = Mathf.Clamp01(_timer / duration);
                glitchMaterial.SetFloat(IntensityId, intensityCurve.Evaluate(t) * _currentMaxIntensity);

                if (t >= 1f)
                {
                    _isGlitching = false;
                    glitchMaterial.SetFloat(IntensityId, 0f);
                }
            }

            // --- Тряска камеры ---
            if (_isShaking && impulseSource != null)
            {
                _shakeTimer += Time.deltaTime;
                float st = Mathf.Clamp01(_shakeTimer / _shakeDurationActive);
                float envelope = shakeEnvelope.Evaluate(st);

                if (randomizeDirection)
                    _shakeDirection = Random.onUnitSphere;

                // Генерируем импульс: направление * сила * огибающая
                impulseSource.GenerateImpulse(_shakeDirection * (shakeAmplitude * envelope));

                if (st >= 1f) _isShaking = false;
            }
        }

        [ProButton]
        /// <summary>Запуск глитча + тряски.</summary>
        public void TriggerGlitch(float intensity = 1f, float overrideDuration = -1f)
        {
            _currentMaxIntensity = Mathf.Clamp01(intensity) * maxIntensity;
            _timer = 0f;
            _isGlitching = true;
            if (overrideDuration > 0) duration = overrideDuration;

            _shakeTimer = 0f;
            _shakeDurationActive = shakeDuration > 0 ? shakeDuration : duration;
            _shakeDirection = Random.onUnitSphere;
            _isShaking = impulseSource != null;
        }

        /// <summary>Только тряска без визуального глитча.</summary>
        public void ShakeOnly(float amplitudeMul = 0.4f, float overrideDuration = 0.2f)
        {
            if (impulseSource == null) return;

            float prevAmp = shakeAmplitude;
            shakeAmplitude *= amplitudeMul;
            _shakeTimer = 0f;
            _shakeDurationActive = overrideDuration > 0 ? overrideDuration : shakeDuration;
            _shakeDirection = Random.onUnitSphere;
            _isShaking = true;
            shakeAmplitude = prevAmp;
        }

        public void StopAll()
        {
            _isGlitching = false;
            _isShaking = false;
            if (glitchMaterial != null) glitchMaterial.SetFloat(IntensityId, 0f);
        }
    }
}
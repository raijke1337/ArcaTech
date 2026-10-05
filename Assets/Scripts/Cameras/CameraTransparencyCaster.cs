using System.Collections.Generic;
using Arcatech.Rendering;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Cameras
{
    /// <summary>
    /// Делает прозрачной геометрию, которая закрывает Теилс от камеры.
    ///
    /// Режимы:
    ///  - Cutout (по умолчанию): шейдеры ARCA вырезают дизеринговую "дыру"
    ///    вокруг Теилс во всём, что между ней и камерой и выше её колен.
    ///    Пол и низ стен остаются, большие объекты не исчезают целиком.
    ///    Работает без MaterialPropertyBlock - SRP Batcher не ломается.
    ///    Участвуют материалы с включённым "Вырезать перед Теилс".
    ///  - WholeObject: старое поведение - объект, попавший в SphereCast,
    ///    растворяется целиком через _FadeAmount (MaterialPropertyBlock).
    ///    Подходит для колонн и мелких объектов; такие рендереры выпадают
    ///    из SRP Batcher, пока растворены.
    ///  - Both: оба сразу.
    /// </summary>
    public class CameraTransparencyCaster : MonoBehaviour
    {
        public enum TransparencyMode { Cutout, WholeObject, Both }

        [Header("Settings")]
        public TransparencyMode mode = TransparencyMode.Cutout;
        public LayerMask wallMask;
        [Tooltip("Радиус SphereCast от камеры к Теилс - определяет, перекрыта ли она.")]
        public float sphereRadius = 0.45f;
        [Tooltip("Скорость появления/исчезновения (доля в секунду).")]
        public float fadeSpeed = 8f;
        [Range(0f, 1f), Tooltip("Сила прозрачности: 1 - почти полностью, 0.85 - остаётся редкий растр.")]
        public float targetFadeAmount = 0.85f;

        [Header("Cutout")]
        [Tooltip("Радиус выреза вокруг луча камера -> Теилс (м).")]
        [Min(0f)] public float cutoutRadius = 1.6f;
        [Tooltip("Мягкость края выреза (м), край сдизерен.")]
        [Min(0.01f)] public float cutoutSoftness = 0.5f;
        [Tooltip("Ниже этой высоты над ступнями Теилс ничего не вырезается (пол, низ стен).")]
        public float cutoutMinHeight = 0.4f;
        [Tooltip("Насколько объект должен быть ближе к камере, чем Теилс, чтобы вырезаться (м).")]
        [Min(0f)] public float cutoutDepthMargin = 0.3f;
        [Tooltip("Вырез всегда включён, даже если SphereCast ничего не нашёл.")]
        public bool cutoutAlwaysOn = false;

        private const int MaxHits = 32;

        private Transform _target;   // точка на теле (EffectSpawn)
        private Transform _feet;     // корень персонажа
        private Camera _cam;

        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];
        private float _cutoutCurrent;

        // WholeObject
        private readonly Dictionary<Collider, Renderer[]> _renderersCache = new();
        private readonly Dictionary<Renderer, MaterialPropertyBlock> _blocks = new();
        private readonly Dictionary<Renderer, float> _currentFade = new();
        private readonly HashSet<Renderer> _hitThisFrame = new();
        private readonly List<Renderer> _keysCache = new();

        private bool UseCutout => mode != TransparencyMode.WholeObject;
        private bool UseWholeObject => mode != TransparencyMode.Cutout;

        private void Awake()
        {
            _cam = Camera.main;
            TryFindPlayer();
        }

        private void OnDisable()
        {
            DisableCutout();
            foreach (var r in _currentFade.Keys)
                if (r != null) r.SetPropertyBlock(null);
            _currentFade.Clear();
            _blocks.Clear();
        }

        private void TryFindPlayer()
        {
            var player = FindAnyObjectByType<PlayerComponent>();
            if (player == null) return;
            _target = player.Entity.EffectSpawn.transform;
            _feet = player.transform;
        }

        private void LateUpdate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_target == null) TryFindPlayer();
            if (_target == null || _cam == null)
            {
                DisableCutout();
                return;
            }

            Vector3 camPos = _cam.transform.position;
            Vector3 toPlayer = _target.position - camPos;
            float dist = toPlayer.magnitude;
            if (dist < 0.001f) return;

            int hitCount = Physics.SphereCastNonAlloc(camPos, sphereRadius, toPlayer / dist, _hits, dist,
                                                      wallMask, QueryTriggerInteraction.Ignore);

            if (UseCutout) UpdateCutout(hitCount > 0 || cutoutAlwaysOn);
            else DisableCutout();

            if (UseWholeObject) UpdateWholeObject(hitCount);
        }

        // ------------------------------------------------------------------
        // Cutout: только глобальные параметры шейдера
        // ------------------------------------------------------------------
        private void UpdateCutout(bool occluded)
        {
            float target = occluded ? cutoutRadius : 0f;
            _cutoutCurrent = Mathf.MoveTowards(_cutoutCurrent, target,
                                               Mathf.Max(cutoutRadius, 0.01f) * fadeSpeed * Time.deltaTime);

            Vector3 player = _target.position;
            Vector3 origin, dir;
            if (_cam.orthographic)
            {
                dir = _cam.transform.forward;
                origin = player - dir * 100f;
            }
            else
            {
                origin = _cam.transform.position;
                dir = (player - origin).normalized;
            }
            float playerT = Vector3.Dot(player - origin, dir);
            float minHeight = (_feet != null ? _feet.position.y : player.y) + cutoutMinHeight;

            Shader.SetGlobalVector(ArcaShaderIds.CutoutRayOrigin, new Vector4(origin.x, origin.y, origin.z, playerT));
            Shader.SetGlobalVector(ArcaShaderIds.CutoutRayDir, new Vector4(dir.x, dir.y, dir.z, _cutoutCurrent));
            Shader.SetGlobalVector(ArcaShaderIds.CutoutParams,
                new Vector4(cutoutSoftness, minHeight, cutoutDepthMargin, targetFadeAmount));
        }

        private void DisableCutout()
        {
            _cutoutCurrent = 0f;
            Shader.SetGlobalVector(ArcaShaderIds.CutoutRayDir, Vector4.zero); // w = 0 -> выключено
        }

        // ------------------------------------------------------------------
        // WholeObject: растворение объекта целиком (старый режим)
        // ------------------------------------------------------------------
        private void UpdateWholeObject(int hitCount)
        {
            _hitThisFrame.Clear();

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _hits[i].collider;
                if (col == null) continue;

                // Кэш вместо GetComponentsInChildren каждый кадр (аллокации).
                if (!_renderersCache.TryGetValue(col, out var renderers))
                {
                    renderers = col.GetComponentsInChildren<Renderer>();
                    _renderersCache[col] = renderers;
                }

                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    _hitThisFrame.Add(r);
                    if (!_currentFade.ContainsKey(r)) _currentFade[r] = 0f;
                }
            }

            _keysCache.Clear();
            _keysCache.AddRange(_currentFade.Keys);

            foreach (var r in _keysCache)
            {
                if (r == null)
                {
                    _currentFade.Remove(r);
                    _blocks.Remove(r);
                    continue;
                }

                bool hit = _hitThisFrame.Contains(r);
                float fade = Mathf.MoveTowards(_currentFade[r], hit ? targetFadeAmount : 0f, fadeSpeed * Time.deltaTime);
                _currentFade[r] = fade;

                if (!hit && fade < 0.01f)
                {
                    // Полностью восстановился - снимаем MPB, рендерер возвращается в SRP Batcher.
                    r.SetPropertyBlock(null);
                    _currentFade.Remove(r);
                    _blocks.Remove(r);
                    continue;
                }

                if (!_blocks.TryGetValue(r, out var mpb))
                {
                    mpb = new MaterialPropertyBlock();
                    _blocks[r] = mpb;
                }
                mpb.SetFloat(ArcaShaderIds.FadeAmount, fade);
                r.SetPropertyBlock(mpb);
            }
        }
    }
}

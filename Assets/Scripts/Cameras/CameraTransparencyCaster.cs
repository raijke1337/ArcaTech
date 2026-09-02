using System.Collections.Generic;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Cameras
{
    public class CameraTransparencyCaster : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask wallMask;
        public float sphereRadius = 0.45f;
        public float fadeSpeed = 8f;
        [Range(0f, 1f)] public float targetFadeAmount = 0.85f; // 0.85 = сильное растворение с обводкой

        private Transform _player;
        private Camera _cam;

        // Используем MaterialPropertyBlock для сохранения SRP Batcher
        private readonly Dictionary<Renderer, MaterialPropertyBlock> _blocks = new();
        private readonly Dictionary<Renderer, float> _currentFade = new();
        private readonly HashSet<Renderer> _hitThisFrame = new();
        private readonly List<Renderer> _keysCache = new();

        // ID свойств шейдера (кэшируем для производительности)
        private static readonly int FadeAmountId = Shader.PropertyToID("_FadeAmount");

        void Awake()
        {
            _cam = Camera.main;
            _player = FindAnyObjectByType<PlayerComponent>()?.transform;
        }

        void LateUpdate()
        {
            if (!_player || !_cam) return;

            _hitThisFrame.Clear();

            Vector3 origin = _cam.transform.position;
            Vector3 toPlayer = _player.position - origin;
            float dist = toPlayer.magnitude;
            if (dist < 0.001f) return;

            var hits = Physics.SphereCastAll(origin, sphereRadius, toPlayer.normalized, dist, wallMask, QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                var renderers = hit.collider.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r != null) _hitThisFrame.Add(r);
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

                float target = _hitThisFrame.Contains(r) ? targetFadeAmount : 0f;
                _currentFade[r] = Mathf.MoveTowards(_currentFade[r], target, fadeSpeed * Time.deltaTime);
                
                ApplyFade(r, _currentFade[r]);

                // Оптимизация: снимаем MPB, если объект полностью видим, чтобы не нагружать рендер
                if (!_hitThisFrame.Contains(r) && _currentFade[r] < 0.01f)
                {
                    r.SetPropertyBlock(null);
                    _currentFade.Remove(r);
                    _blocks.Remove(r);
                }
            }
        }

        void ApplyFade(Renderer r, float fade)
        {
            if (!_blocks.TryGetValue(r, out var mpb))
            {
                mpb = new MaterialPropertyBlock();
                _blocks[r] = mpb;
            }
            mpb.SetFloat(FadeAmountId, fade);
            r.SetPropertyBlock(mpb);
        }
    }
}
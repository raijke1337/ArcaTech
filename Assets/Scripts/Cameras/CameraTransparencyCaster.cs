using System.Collections.Generic;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Cameras
{
    public class CameraTransparencyCaster : MonoBehaviour
    {
        Transform player;
        public LayerMask wallMask;
        public float sphereRadius = 0.45f;
        public float fadeSpeed = 8f;
        [Range(0f, 1f)] public float targetAlpha = 0.25f;

        Camera cam;

        readonly Dictionary<Renderer, float> currentAlpha = new();
        readonly Dictionary<Renderer, Color> originalColor = new(); // RGB больше не трогаем
        readonly Dictionary<Renderer, MaterialPropertyBlock> blocks = new();
        readonly HashSet<Renderer> hitThisFrame = new();
        readonly List<Renderer> keysCache = new();

        static readonly int ColorId = Shader.PropertyToID("_Color");

        void Awake()
        {
            cam = Camera.main;
            player = FindAnyObjectByType<PlayerComponent>().transform;
        }

        void LateUpdate()
        {
            if (!player || !cam) return;

            hitThisFrame.Clear();

            Vector3 origin = cam.transform.position;
            Vector3 toPlayer = player.position - origin;
            float dist = toPlayer.magnitude;
            if (dist < 0.001f) return;

            var hits = Physics.SphereCastAll(
                origin, sphereRadius, toPlayer.normalized, dist, wallMask,
                QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                var renderers = hit.collider.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (!r) continue;
                    hitThisFrame.Add(r);
                    EnsureTracked(r);
                }
            }

            keysCache.Clear();
            keysCache.AddRange(currentAlpha.Keys);

            foreach (var r in keysCache)
            {
                if (!r)
                {
                    currentAlpha.Remove(r);
                    originalColor.Remove(r);
                    blocks.Remove(r);
                    continue;
                }

                float target = hitThisFrame.Contains(r) ? targetAlpha : 1f;
                currentAlpha[r] = Mathf.MoveTowards(currentAlpha[r], target, fadeSpeed * Time.deltaTime);
                ApplyAlpha(r, currentAlpha[r]);

                // Полностью восстановились — снимаем override
                if (!hitThisFrame.Contains(r) && Mathf.Abs(currentAlpha[r] - 1f) < 0.001f)
                {
                    r.SetPropertyBlock(null);
                    currentAlpha.Remove(r);
                    originalColor.Remove(r);
                    blocks.Remove(r);
                }
            }
        }

        void EnsureTracked(Renderer r)
        {
            if (currentAlpha.ContainsKey(r)) return;

            currentAlpha[r] = 1f;

            // Один раз читаем исходный цвет с материала (не из MPB!)
            Color baseColor = Color.white;
            var mat = r.sharedMaterial;
            if (mat != null && mat.HasProperty(ColorId))
                baseColor = mat.GetColor(ColorId);

            originalColor[r] = baseColor;
        }

        void ApplyAlpha(Renderer r, float a)
        {
            if (!originalColor.TryGetValue(r, out var baseColor))
                return;

            if (!blocks.TryGetValue(r, out var mpb))
            {
                mpb = new MaterialPropertyBlock();
                blocks[r] = mpb;
            }

            // Всегда берём закешированный RGB, меняем только A
            Color c = baseColor;
            c.a = baseColor.a * a; // учитываем alpha материала, если она не 1
            mpb.SetColor(ColorId, c);
            r.SetPropertyBlock(mpb);
        }
    }
}
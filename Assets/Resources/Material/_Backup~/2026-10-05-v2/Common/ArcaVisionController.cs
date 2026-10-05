using UnityEngine;

namespace Arcatech.Rendering
{
    /// <summary>
    /// "Кошачье зрение" Теилс: задаёт глобальные параметры для ARCA-шейдеров
    /// (ARCAToonLighting.hlsl -> ArcaVisionFactor / ArcaApplyCatVision).
    ///
    /// Вешается на Теилс (или на любой объект с ссылкой на неё). Пока компонент
    /// выключен или его нет на сцене, шейдеры рисуют мир без ограничения обзора.
    ///
    /// Рядом с Теилс - полный цвет. К краю обзора цвета уходят в сине-жёлтую гамму
    /// (кошки - дихроматы) и темнеют; дальше видны только силуэты, подсвеченная
    /// обводка и светящиеся объекты.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ArcaVisionController : MonoBehaviour
    {
        [Header("Цель")]
        [Tooltip("Центр обзора. Пусто - этот объект.")]
        [SerializeField] private Transform target;

        [Header("Радиус")]
        [Tooltip("Радиус полного зрения (м). Внутри мир в полном цвете.")]
        [SerializeField, Min(0f)] private float radius = 7f;
        [Tooltip("Ширина перехода к темноте (м).")]
        [SerializeField, Min(0.01f)] private float transitionWidth = 6f;
        [Tooltip("Количество ступеней перехода (toon). 1 - резкая граница, 16+ - почти плавно.")]
        [SerializeField, Range(1, 16)] private int bands = 4;
        [Tooltip("Сила дизеринга между ступенями: 0 - чистые ступени, 1 - Bayer-растр.")]
        [SerializeField, Range(0f, 1f)] private float ditherStrength = 1f;

        [Header("Ночь")]
        [Tooltip("Яркость мира за пределами обзора (доля от освещения).")]
        [SerializeField, Range(0f, 1f)] private float nightBrightness = 0.18f;
        [Tooltip("Оттенок мира за пределами обзора. По умолчанию холодный синий.")]
        [SerializeField] private Color nightTint = new Color(0.62f, 0.72f, 1.0f, 1f);
        [Tooltip("Сила кошачьей дихромазии на краю обзора (красный/зелёный сливаются).")]
        [SerializeField, Range(0f, 1f)] private float dichromacy = 1f;
        [Tooltip("Какая доля эмиссии остаётся видна в темноте.")]
        [SerializeField, Range(0f, 1f)] private float emissionInDarkness = 0.6f;
        [Tooltip("Цвет обводки в темноте - по нему читаются силуэты.")]
        [SerializeField] private Color silhouetteColor = new Color(0.23f, 0.29f, 0.42f, 1f); // ~#3A4A6B

        [Header("Свет Теилс (опционально)")]
        [Tooltip("Point Light на Теилс. Его range подстраивается под радиус зрения.")]
        [SerializeField] private Light visionLight;
        [SerializeField, Min(0f)] private float lightRangeMultiplier = 1.2f;

        private static readonly int VisionCenterId     = Shader.PropertyToID("_ArcaVisionCenter");
        private static readonly int VisionParamsId     = Shader.PropertyToID("_ArcaVisionParams");
        private static readonly int VisionParams2Id    = Shader.PropertyToID("_ArcaVisionParams2");
        private static readonly int NightTintId        = Shader.PropertyToID("_ArcaNightTint");
        private static readonly int SilhouetteColorId  = Shader.PropertyToID("_ArcaSilhouetteColor");

        /// <summary>Радиус можно менять из геймплея (вспышка, фонарь, перегрузка).</summary>
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0f, value);
        }

        private Transform Center => target != null ? target : transform;

        private void OnEnable() => Push();

        private void LateUpdate() => Push();

        private void OnValidate() => Push();

        private void OnDisable()
        {
            // z = 0 -> шейдеры выключают зрение, мир снова виден целиком.
            Shader.SetGlobalVector(VisionParamsId, Vector4.zero);
        }

        private void Push()
        {
            if (!isActiveAndEnabled)
                return;

            Vector3 p = Center.position;
            Shader.SetGlobalVector(VisionCenterId, new Vector4(p.x, p.y, p.z, radius));
            Shader.SetGlobalVector(VisionParamsId, new Vector4(transitionWidth, ditherStrength, 1f, nightBrightness));
            Shader.SetGlobalVector(VisionParams2Id, new Vector4(dichromacy, emissionInDarkness, bands, 0f));
            Shader.SetGlobalColor(NightTintId, nightTint);
            Shader.SetGlobalColor(SilhouetteColorId, silhouetteColor);

            if (visionLight != null)
                visionLight.range = (radius + transitionWidth * 0.5f) * lightRangeMultiplier;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 p = Center.position;
            Gizmos.color = new Color(1f, 0.72f, 0.29f, 1f);          // полный обзор
            DrawCircle(p, radius);
            Gizmos.color = new Color(0.27f, 0.48f, 1f, 1f);          // конец перехода
            DrawCircle(p, radius + transitionWidth);
        }

        private static void DrawCircle(Vector3 center, float r)
        {
            const int segments = 48;
            Vector3 prev = center + new Vector3(r, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                Vector3 next = center + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
}

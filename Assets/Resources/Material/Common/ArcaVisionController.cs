using UnityEngine;

namespace Arcatech.Rendering
{
    /// <summary>
    /// "Кошачье зрение" Теилс: центр и радиус обзора для ARCA-шейдеров.
    /// Стиль темноты (переход, оттенок, силуэты) задаётся профилем биома
    /// через <see cref="ArcaWorldLook"/>; без него используются значения
    /// по умолчанию.
    ///
    /// Вешается на Теилс. Пока компонент выключен или его нет, мир виден целиком.
    ///
    /// Файл можно перенести в Scripts/Rendering средствами Unity (вместе с .meta
    /// ссылки на сцене сохранятся).
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ArcaVisionController : MonoBehaviour
    {
        [Tooltip("Центр обзора. Пусто - этот объект.")]
        [SerializeField] private Transform target;

        [Tooltip("Радиус полного зрения (м). Внутри мир в полном цвете.")]
        [SerializeField, Min(0f)] private float radius = 7f;

        [Header("Свет Теилс (опционально)")]
        [Tooltip("Point Light на Теилс. Его Range подстраивается под радиус зрения; " +
                 "Intensity задаёт, насколько далеко внутри Range дотягивается пятно света.")]
        [SerializeField] private Light visionLight;
        [SerializeField, Min(0f)] private float lightRangeMultiplier = 1.2f;

        /// <summary>Радиус можно менять из геймплея (вспышка, перегрузка, тёмная зона).</summary>
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0f, value);
        }

        private Transform Center => target != null ? target : transform;

        private static ArcaNightLook CurrentNight =>
            ArcaWorldLook.Active != null && ArcaWorldLook.Active.Profile != null
                ? ArcaWorldLook.Active.Profile.night
                : ArcaNightLook.Default;

        private void OnEnable() => Push();
        private void LateUpdate() => Push();
        private void OnValidate() => Push();

        private void OnDisable()
        {
            // w = 0 -> шейдеры выключают зрение
            Shader.SetGlobalVector(ArcaShaderIds.VisionCenter, Vector4.zero);
        }

        private void Push()
        {
            if (!isActiveAndEnabled) return;

            Vector3 p = Center.position;
            Shader.SetGlobalVector(ArcaShaderIds.VisionCenter, new Vector4(p.x, p.y, p.z, radius));

            ArcaNightLook night = CurrentNight;

            // Без профиля биома стиль темноты никто не задаёт - задаём сами.
            if (ArcaWorldLook.Active == null)
                night.Push();

            if (visionLight != null)
                visionLight.range = (radius + night.transitionWidth * 0.5f) * lightRangeMultiplier;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 p = Center.position;
            Gizmos.color = new Color(1f, 0.72f, 0.29f, 1f);          // полный обзор
            DrawCircle(p, radius);
            Gizmos.color = new Color(0.27f, 0.48f, 1f, 1f);          // конец перехода
            DrawCircle(p, radius + CurrentNight.transitionWidth);
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

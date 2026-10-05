using UnityEngine;

namespace Arcatech.Rendering
{
    /// <summary>
    /// Применяет <see cref="ArcaLookProfile"/> к сцене: ambient для ARCA-шейдеров,
    /// главный свет, туман и стиль "кошачьего зрения".
    ///
    /// Один компонент на сцену уровня. Сменить профиль на лету (зал с
    /// аварийным светом, босс-арена) - <see cref="SetProfile"/> с длительностью
    /// перехода.
    ///
    /// Если профиль включает "Главный свет", компонент сам выставляет цвет и
    /// интенсивность directional-света - править их нужно в профиле.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ArcaWorldLook : MonoBehaviour
    {
        public static ArcaWorldLook Active { get; private set; }

        [SerializeField] private ArcaLookProfile profile;
        [Tooltip("Directional-свет уровня. Пусто - Lighting > Sun Source.")]
        [SerializeField] private Light mainLight;

        private ArcaLookProfile _from;
        private float _blend = 1f;
        private float _blendDuration;

        public ArcaLookProfile Profile => profile;

        private void OnEnable()
        {
            if (Active != null && Active != this)
                Debug.LogWarning($"[ArcaWorldLook] На сцене несколько ArcaWorldLook, активен '{name}'.", this);
            Active = this;
            _blend = 1f;
            Apply();
        }

        private void OnDisable()
        {
            if (Active != this) return;
            Active = null;
            // a = 0 -> шейдеры снова берут Environment Lighting сцены
            Shader.SetGlobalColor(ArcaShaderIds.AmbientSky, Color.clear);
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            _blend = 1f;
            Apply();
        }

        private void Update()
        {
            if (_blend >= 1f) return;
            _blend = _blendDuration <= 0f ? 1f : Mathf.Min(1f, _blend + Time.deltaTime / _blendDuration);
            Apply();
        }

        /// <summary>Сменить профиль. duration > 0 - плавный переход (сек).</summary>
        public void SetProfile(ArcaLookProfile newProfile, float duration = 0f)
        {
            if (newProfile == null) return;
            _from = duration > 0f ? profile : null;
            profile = newProfile;
            _blendDuration = duration;
            _blend = duration > 0f ? 0f : 1f;
            Apply();
        }

        private void Apply()
        {
            if (profile == null) return;

            ArcaLookProfile a = _from != null ? _from : profile;
            ArcaLookProfile b = profile;
            float t = Mathf.SmoothStep(0f, 1f, _blend);

            // --- Ambient ---
            Color sky = Color.Lerp(a.ambientSky, b.ambientSky, t);
            sky.a = b.overrideAmbient ? 1f : 0f;
            Shader.SetGlobalColor(ArcaShaderIds.AmbientSky, sky);
            Shader.SetGlobalColor(ArcaShaderIds.AmbientEquator, Color.Lerp(a.ambientEquator, b.ambientEquator, t));
            Shader.SetGlobalColor(ArcaShaderIds.AmbientGround, Color.Lerp(a.ambientGround, b.ambientGround, t));

            // --- Главный свет ---
            Light sun = mainLight != null ? mainLight : RenderSettings.sun;
            if (b.overrideMainLight && sun != null)
            {
                sun.color = Color.Lerp(a.mainLightColor, b.mainLightColor, t);
                sun.intensity = Mathf.Lerp(a.mainLightIntensity, b.mainLightIntensity, t);
            }

            // --- Туман ---
            if (b.overrideFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = b.fogMode;
                RenderSettings.fogColor = Color.Lerp(a.fogColor, b.fogColor, t);
                RenderSettings.fogStartDistance = Mathf.Lerp(a.fogStart, b.fogStart, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(a.fogEnd, b.fogEnd, t);
                RenderSettings.fogDensity = Mathf.Lerp(a.fogDensity, b.fogDensity, t);
            }

            // --- Кошачье зрение ---
            ArcaNightLook.Lerp(a.night, b.night, t).Push();

            if (_blend >= 1f) _from = null;
        }
    }
}

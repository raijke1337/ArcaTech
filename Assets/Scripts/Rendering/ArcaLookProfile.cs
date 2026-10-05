using System;
using UnityEngine;

namespace Arcatech.Rendering
{
    /// <summary>
    /// Визуальный профиль биома/уровня: ambient, главный свет, туман и стиль
    /// "кошачьего зрения". Один ассет на биом; применяется компонентом
    /// <see cref="ArcaWorldLook"/>. Переходы между профилями (например, при
    /// входе в освещённый зал) - через ArcaWorldLook.SetProfile(profile, duration).
    /// </summary>
    [CreateAssetMenu(menuName = "Arcatech/Rendering/Look Profile", fileName = "ArcaLook_New")]
    public sealed class ArcaLookProfile : ScriptableObject
    {
        [Header("Ambient (вместо Environment Lighting сцены)")]
        [Tooltip("Выключено - шейдеры берут Environment Lighting из окна Lighting.")]
        public bool overrideAmbient = true;
        [ColorUsage(false, true)] public Color ambientSky     = new Color(0.106f, 0.133f, 0.188f); // #1B2230
        [ColorUsage(false, true)] public Color ambientEquator = new Color(0.063f, 0.075f, 0.114f); // #10131D
        [ColorUsage(false, true)] public Color ambientGround  = new Color(0.039f, 0.047f, 0.071f); // #0A0C12

        [Header("Главный свет")]
        public bool overrideMainLight = true;
        public Color mainLightColor = new Color(0.62f, 0.72f, 1.0f);   // холодный, около #9EB8FF
        [Min(0f)] public float mainLightIntensity = 0.25f;

        [Header("Туман")]
        public bool overrideFog = false;
        public Color fogColor = new Color(0.063f, 0.075f, 0.114f);       // #10131D
        public FogMode fogMode = FogMode.Linear;
        [Min(0f)] public float fogStart = 20f;
        [Min(0f)] public float fogEnd = 60f;
        [Min(0f)] public float fogDensity = 0.02f;

        [Header("Кошачье зрение")]
        public ArcaNightLook night = new ArcaNightLook();
    }

    /// <summary>Как выглядит мир за пределами обзора Теилс.</summary>
    [Serializable]
    public sealed class ArcaNightLook
    {
        [Tooltip("Ширина перехода от полного зрения к темноте (м).")]
        [Min(0.01f)] public float transitionWidth = 6f;
        [Tooltip("Ступени перехода (toon). 1 - резкая граница, 16 - почти плавно.")]
        [Range(1, 16)] public int bands = 4;
        [Tooltip("0 - чистые ступени, 1 - Bayer-растр между ними.")]
        [Range(0f, 1f)] public float ditherStrength = 1f;
        [Tooltip("Яркость мира за пределами обзора (доля освещения).")]
        [Range(0f, 1f)] public float nightBrightness = 0.18f;
        [Tooltip("Оттенок мира за пределами обзора.")]
        public Color nightTint = new Color(0.62f, 0.72f, 1.0f);
        [Tooltip("Кошачья дихромазия на краю обзора: красный и зелёный сливаются.")]
        [Range(0f, 1f)] public float dichromacy = 1f;
        [Tooltip("Доля эмиссии, видимая в темноте.")]
        [Range(0f, 1f)] public float emissionInDarkness = 0.6f;
        [Tooltip("Цвет обводки в темноте - по нему читаются силуэты.")]
        public Color silhouetteColor = new Color(0.23f, 0.29f, 0.42f);   // ~#3A4A6B

        public static readonly ArcaNightLook Default = new ArcaNightLook();

        public void Push()
        {
            Shader.SetGlobalVector(ArcaShaderIds.VisionStyle,
                new Vector4(transitionWidth, ditherStrength, bands, nightBrightness));
            Shader.SetGlobalVector(ArcaShaderIds.VisionStyle2,
                new Vector4(dichromacy, emissionInDarkness, 0f, 0f));
            Shader.SetGlobalColor(ArcaShaderIds.NightTint, nightTint);
            Shader.SetGlobalColor(ArcaShaderIds.SilhouetteColor, silhouetteColor);
        }

        public static ArcaNightLook Lerp(ArcaNightLook a, ArcaNightLook b, float t)
        {
            return new ArcaNightLook
            {
                transitionWidth    = Mathf.Lerp(a.transitionWidth, b.transitionWidth, t),
                bands              = Mathf.RoundToInt(Mathf.Lerp(a.bands, b.bands, t)),
                ditherStrength     = Mathf.Lerp(a.ditherStrength, b.ditherStrength, t),
                nightBrightness    = Mathf.Lerp(a.nightBrightness, b.nightBrightness, t),
                nightTint          = Color.Lerp(a.nightTint, b.nightTint, t),
                dichromacy         = Mathf.Lerp(a.dichromacy, b.dichromacy, t),
                emissionInDarkness = Mathf.Lerp(a.emissionInDarkness, b.emissionInDarkness, t),
                silhouetteColor    = Color.Lerp(a.silhouetteColor, b.silhouetteColor, t),
            };
        }
    }
}

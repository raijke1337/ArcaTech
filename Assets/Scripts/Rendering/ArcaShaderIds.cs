using UnityEngine;

namespace Arcatech.Rendering
{
    /// <summary>
    /// ID глобальных параметров ARCA-шейдеров (Material/Common/ArcaGlobals.hlsl).
    /// Единый источник имён: при переименовании в HLSL правится только здесь.
    /// </summary>
    public static class ArcaShaderIds
    {
        // Кошачье зрение
        public static readonly int VisionCenter     = Shader.PropertyToID("_ArcaVisionCenter");
        public static readonly int VisionStyle      = Shader.PropertyToID("_ArcaVisionStyle");
        public static readonly int VisionStyle2     = Shader.PropertyToID("_ArcaVisionStyle2");
        public static readonly int NightTint        = Shader.PropertyToID("_ArcaNightTint");
        public static readonly int SilhouetteColor  = Shader.PropertyToID("_ArcaSilhouetteColor");

        // Ambient биома
        public static readonly int AmbientSky       = Shader.PropertyToID("_ArcaAmbientSky");
        public static readonly int AmbientEquator   = Shader.PropertyToID("_ArcaAmbientEquator");
        public static readonly int AmbientGround    = Shader.PropertyToID("_ArcaAmbientGround");

        // Вырез перед Теилс
        public static readonly int CutoutRayOrigin  = Shader.PropertyToID("_ArcaCutoutRayOrigin");
        public static readonly int CutoutRayDir     = Shader.PropertyToID("_ArcaCutoutRayDir");
        public static readonly int CutoutParams     = Shader.PropertyToID("_ArcaCutoutParams");

        // Фонарь Теилс
        public static readonly int LanternPosition  = Shader.PropertyToID("_ArcaLanternPosition");

        // Материал
        public static readonly int FadeAmount       = Shader.PropertyToID("_FadeAmount");
        public static readonly int DamageFlashStrength = Shader.PropertyToID("_DamageFlashStrength");
        public static readonly int DamageFlashColor    = Shader.PropertyToID("_DamageFlashColor");
        public static readonly int DamageFlashDetail   = Shader.PropertyToID("_DamageFlashDetail");
        public static readonly int DamageFlashHalftone = Shader.PropertyToID("_DamageFlashHalftone");
        public static readonly int DamageFlashEmission = Shader.PropertyToID("_DamageFlashEmission");
    }
}

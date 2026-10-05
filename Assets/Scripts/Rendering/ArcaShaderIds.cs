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

        // Материал
        public static readonly int FadeAmount       = Shader.PropertyToID("_FadeAmount");
    }
}

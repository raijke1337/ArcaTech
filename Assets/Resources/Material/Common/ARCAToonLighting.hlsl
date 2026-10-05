#ifndef ARCA_TOON_LIGHTING_INCLUDED
#define ARCA_TOON_LIGHTING_INCLUDED

// ===========================================================================
// УСТАРЕВШИЙ входной файл - оставлен для совместимости.
// Новые шейдеры подключают модули напрямую:
//   ArcaGlobals.hlsl   - глобальные параметры (зрение, ambient биома, вырез)
//   ARCADither.hlsl    - Bayer-дизеринг
//   ArcaVision.hlsl    - кошачье зрение
//   ArcaOcclusion.hlsl - прозрачность перекрывающей геометрии
//   ArcaLighting.hlsl  - toon-освещение (нужен Lighting.hlsl)
//   ArcaOutline.hlsl   - обводка с толщиной в пикселях
//   ArcaPasses.hlsl    - ShadowCaster / DepthOnly / DepthNormals
// ===========================================================================

#include "ArcaGlobals.hlsl"
#include "ARCADither.hlsl"
#include "ArcaVision.hlsl"

#if defined(UNIVERSAL_LIGHTING_INCLUDED)
#include "ArcaLighting.hlsl"

// Старая сигнатура. shadowCoord игнорируется: координата тени теперь
// всегда считается попиксельно внутри ArcaToonLighting.
half3 ApplyArcaToonLightingEx(half3 albedo, float4 positionCS, float3 positionWS,
                              float3 normalWS, float4 shadowCoord, ArcaToonParams p)
{
    ArcaSurface s;
    s.albedo     = albedo;
    s.occlusion  = 1.0;
    s.positionWS = positionWS;
    s.normalWS   = normalWS;
    s.positionCS = positionCS;
    return ArcaToonLighting(s, p);
}
#endif

#endif // ARCA_TOON_LIGHTING_INCLUDED

#ifndef ARCA_PASSES_INCLUDED
#define ARCA_PASSES_INCLUDED

// ===========================================================================
// Общие служебные пассы ARCA-шейдеров: ShadowCaster, DepthOnly, DepthNormals.
// Подключается внутри HLSLPROGRAM пасса после объявления CBUFFER шейдера.
//
//   ShadowCaster: #pragma vertex ArcaShadowVert   / fragment ArcaShadowFrag
//   DepthOnly:    #pragma vertex ArcaDepthVert    / fragment ArcaDepthFrag
//   DepthNormals: #pragma vertex ArcaDepthVert    / fragment ArcaDepthNormalsFrag
//
// Прозрачность (ArcaOcclusion.hlsl) учитывается во всех пассах; вырез перед
// Теилс - везде, кроме теней (тень не должна зависеть от камеры).
// DepthNormals нужен SSAO: без него на ARCA-объектах были артефакты.
//
// #define ARCA_LANTERN_SHADOW_SKIP (+ _ArcaNoLanternShadow в CBUFFER) - материал
// умеет не отбрасывать тень от фонаря Теилс (см. ArcaGlobals.hlsl).
// ===========================================================================

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "ArcaGlobals.hlsl"
#include "ARCADither.hlsl"
#include "ArcaOcclusion.hlsl"

struct ArcaPassAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
};

struct ArcaPassVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 normalWS   : TEXCOORD1;
    float  localX     : TEXCOORD2;
};

// --- ShadowCaster ----------------------------------------------------------
float3 _LightDirection;
float3 _LightPosition;

ArcaPassVaryings ArcaShadowVert(ArcaPassAttributes input)
{
    ArcaPassVaryings output;
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS   = TransformObjectToWorldNormal(input.normalOS);

    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
        float3 lightDirectionWS = normalize(_LightPosition - positionWS);
    #else
        float3 lightDirectionWS = _LightDirection;
    #endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    #if UNITY_REVERSED_Z
        positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
        positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif

    // Не отбрасывать тень от фонаря Теилс (материалы с _ArcaNoLanternShadow).
    // Во время отрисовки теней точечного света _LightPosition = позиция этой
    // лампы: если это фонарь, вершины уводятся за пределы экрана тени и
    // треугольник отбрасывается ещё до растеризации.
    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW) && defined(ARCA_LANTERN_SHADOW_SKIP)
        if (_ArcaNoLanternShadow > 0.5 && _ArcaLanternPosition.w > 0.5 &&
            distance(_LightPosition, _ArcaLanternPosition.xyz) < 0.05)
        {
            positionCS = float4(2.0, 2.0, 2.0, 1.0);
        }
    #endif

    output.positionCS = positionCS;
    output.positionWS = positionWS;
    output.normalWS   = normalWS;
    output.localX     = input.positionOS.x;
    return output;
}

half4 ArcaShadowFrag(ArcaPassVaryings input) : SV_TARGET
{
    ArcaClipOcclusion(input.positionCS, input.positionWS, input.localX, false);
    return 0;
}

// --- DepthOnly / DepthNormals ---------------------------------------------
ArcaPassVaryings ArcaDepthVert(ArcaPassAttributes input)
{
    ArcaPassVaryings output;
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
    output.localX     = input.positionOS.x;
    return output;
}

half ArcaDepthFrag(ArcaPassVaryings input) : SV_TARGET
{
    ArcaClipOcclusion(input.positionCS, input.positionWS, input.localX, true);
    return input.positionCS.z;
}

half4 ArcaDepthNormalsFrag(ArcaPassVaryings input) : SV_TARGET
{
    ArcaClipOcclusion(input.positionCS, input.positionWS, input.localX, true);

    float3 normalWS = normalize(input.normalWS);
    #if defined(_GBUFFER_NORMALS_OCT)
        float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
        float2 remapped    = saturate(octNormalWS * 0.5 + 0.5);
        return half4(PackFloat2To888(remapped), 0.0);
    #else
        return half4(normalWS, 0.0);
    #endif
}

#endif // ARCA_PASSES_INCLUDED

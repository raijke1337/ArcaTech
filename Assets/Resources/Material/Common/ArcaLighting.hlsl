#ifndef ARCA_LIGHTING_INCLUDED
#define ARCA_LIGHTING_INCLUDED

// ===========================================================================
// Единая toon-модель освещения ARCA: окружение и персонажи.
//  - главный свет: ступень + жёсткая тень, цветная тень = доля того же света;
//  - ambient: градиент профиля биома (ArcaWorldLook) или Environment Lighting;
//  - локальные источники: две ступени (ореол + ядро). Intensity лампы задаёт
//    РАЗМЕР пятна, цвет - оттенок, яркость внутри пятна постоянна;
//  - ступенчатый specular (для металлов), rim, halftone в тени, SSAO.
//
// Требует, чтобы шейдер подключил Lighting.hlsl и объявил pragma:
//   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
//   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
//   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
//   #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
//   #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
//   #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
//   #pragma multi_compile_fog
// ===========================================================================

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"

struct ArcaSurface
{
    half3  albedo;
    half   occlusion;    // 1 - без затенения (карта AO материала)
    float3 positionWS;
    float3 normalWS;
    float4 positionCS;
};

struct ArcaToonParams
{
    float  stepThreshold;
    float  stepSmooth;
    half3  shadowColor;       // доля света в тени (цветная, не чёрная)
    float  ambientInfluence;  // вклад ambient
    float  addLightStep;      // порог первой ступени для point/spot
    half3  rimColor;
    float  rimThreshold;
    float  rimPower;
    float  halftoneScale;     // ячеек по высоте экрана
    float  halftoneStrength;  // 0..1
    half3  specColor;         // ступенчатый блик (металл)
    float  specSize;          // 0 - выключен, 0.05..0.4 - размер блика
};

ArcaToonParams ArcaDefaultToonParams()
{
    ArcaToonParams p;
    p.stepThreshold    = 0.5;
    p.stepSmooth       = 0.02;
    p.shadowColor      = half3(0.35, 0.33, 0.5);
    p.ambientInfluence = 0.5;
    p.addLightStep     = 0.3;
    p.rimColor         = half3(0, 0, 0);
    p.rimThreshold     = 0.6;
    p.rimPower         = 3.0;
    p.halftoneScale    = 80.0;
    p.halftoneStrength = 0.0;
    p.specColor        = half3(0, 0, 0);
    p.specSize         = 0.0;
    return p;
}

// Ambient: градиент профиля биома (если задан) или Environment Lighting сцены.
half3 ArcaAmbient(float3 normalWS)
{
    if (_ArcaAmbientSky.a > 0.5)
    {
        half up = (half)normalWS.y;
        return up >= 0.0
            ? lerp(_ArcaAmbientEquator.rgb, _ArcaAmbientSky.rgb, up)
            : lerp(_ArcaAmbientEquator.rgb, _ArcaAmbientGround.rgb, -up);
    }
    return SampleSH(normalWS);
}

half3 ArcaToonLighting(ArcaSurface s, ArcaToonParams p)
{
    float3 normalWS = normalize(s.normalWS);
    float2 screenUV = GetNormalizedScreenSpaceUV(s.positionCS);

    // --- SSAO ---
    half directAO   = 1.0;
    half indirectAO = s.occlusion;
    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor ao = GetScreenSpaceAmbientOcclusion(screenUV);
        directAO    = ao.directAmbientOcclusion;
        indirectAO *= ao.indirectAmbientOcclusion;
    #endif

    // --- главный свет ---
    // Координата тени - попиксельно (при каскадах по вершине - артефакты).
    float4 shadowCoord = TransformWorldToShadowCoord(s.positionWS);
    Light  mainLight   = GetMainLight(shadowCoord);
    half3  mainColor   = mainLight.color * directAO;
    float  NdotL       = saturate(dot(normalWS, mainLight.direction));

    float litStep    = smoothstep(p.stepThreshold - p.stepSmooth, p.stepThreshold + p.stepSmooth, NdotL);
    float shadowStep = smoothstep(0.4, 0.6, mainLight.shadowAttenuation); // жёсткая тень
    float lit        = litStep * shadowStep;

    // --- цветная тень: доля того же света + ambient ---
    half3 ambient   = ArcaAmbient(normalWS) * p.ambientInfluence * indirectAO;
    half3 shadowCol = s.albedo * (mainColor * p.shadowColor * s.occlusion + ambient);
    half3 litCol    = s.albedo * (mainColor + ambient);
    half3 col       = lerp(shadowCol, litCol, lit);

    // --- локальные источники ---
    #if defined(_ADDITIONAL_LIGHTS) || defined(_CLUSTER_LIGHT_LOOP) || defined(_FORWARD_PLUS)
        InputData inputData = (InputData)0;
        inputData.positionWS = s.positionWS;
        inputData.normalWS = normalWS;
        inputData.normalizedScreenSpaceUV = screenUV;
        half4 shadowMask = half4(1, 1, 1, 1);

        float step2 = lerp(p.addLightStep, 1.0, 0.5);

        uint pixelLightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light l = GetAdditionalLight(lightIndex, s.positionWS, shadowMask);
            half  lum = max(dot(l.color, half3(0.299, 0.587, 0.114)), 0.0001);
            // Тень от лампы (Shadow Type на Light + _ADDITIONAL_LIGHT_SHADOWS) - жёсткая, toon.
            float lsh = smoothstep(0.4, 0.6, l.shadowAttenuation);
            float a   = saturate(dot(normalWS, l.direction)) * l.distanceAttenuation * lsh * lum;
            float s1  = smoothstep(p.addLightStep - p.stepSmooth, p.addLightStep + p.stepSmooth, a);
            float s2  = smoothstep(step2 - p.stepSmooth, step2 + p.stepSmooth, a);
            col += s.albedo * (l.color / lum) * (s1 * 0.5 + s2 * 0.5) * directAO;
        LIGHT_LOOP_END
    #endif

    // --- halftone в тени: точки растут от границы тени вглубь ---
    if (p.halftoneStrength > 0.001)
    {
        float cellPx  = max(_ScreenParams.y / max(p.halftoneScale, 1.0), 2.0);
        float2 px     = s.positionCS.xy;
        float2 rot    = float2(px.x + px.y, px.x - px.y) * 0.70710678; // поворот 45°
        float2 cell   = frac(rot / cellPx) - 0.5;
        float depth   = saturate(1.0 - NdotL / max(p.stepThreshold, 0.001));
        depth         = max(depth, (1.0 - shadowStep) * 0.5);
        float dots    = step(length(cell), depth * 0.75);
        col = lerp(col, shadowCol * 0.65, dots * (1.0 - lit) * p.halftoneStrength);
    }

    half3 viewDir = GetWorldSpaceNormalizeViewDir(s.positionWS);

    // --- ступенчатый блик (металл) ---
    if (p.specSize > 0.0001)
    {
        float3 h     = normalize(mainLight.direction + viewDir);
        float  NdotH = saturate(dot(normalWS, h));
        float  edge  = 1.0 - p.specSize * p.specSize;   // квадрат - удобнее крутить ползунок
        float  spec  = smoothstep(edge - p.stepSmooth * 0.5, edge + p.stepSmooth * 0.5, NdotH) * lit;
        col += p.specColor * mainColor * spec;
    }

    // --- rim со стороны key-света, масштабируется самим светом ---
    float fres = pow(1.0 - saturate(dot(normalWS, viewDir)), p.rimPower);
    float rim  = smoothstep(p.rimThreshold, p.rimThreshold + 0.02, fres) * saturate(NdotL + 0.3);
    col += rim * p.rimColor * mainColor;

    return col;
}

#endif // ARCA_LIGHTING_INCLUDED

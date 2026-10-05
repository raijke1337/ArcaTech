#ifndef ARCA_TOON_LIGHTING_INCLUDED
#define ARCA_TOON_LIGHTING_INCLUDED

// Общий toon-степ + halftone-оверлей для непрозрачных ARCA toon-шейдеров.
// Требует, чтобы вызывающий шейдер подключил Lighting.hlsl и объявил pragma:
//   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
//   #pragma multi_compile _ _SHADOWS_SOFT
//   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
//   #pragma multi_compile _ _FORWARD_PLUS
//   #pragma multi_compile_fog

// ---------------------------------------------------------------------------
// СТАРАЯ ВЕРСИЯ (без изменений) - для шейдеров, которые ещё не переведены
// ---------------------------------------------------------------------------
half3 ApplyArcaToonLighting(half3 baseColor, float4 positionCS, float3 normalWS,
                             float4 shadowCoord, float stepThreshold,
                             float halftoneScale, float halftoneStrength)
{
    Light mainLight = GetMainLight(shadowCoord);

    float NdotL = max(0.0, dot(normalWS, mainLight.direction));
    float stepLight = smoothstep(stepThreshold - 0.05, stepThreshold + 0.05, NdotL);
    stepLight *= mainLight.shadowAttenuation;

    float2 screenUV = positionCS.xy * halftoneScale / _ScreenParams.xy;
    float halftonePattern = sin(screenUV.x) * sin(screenUV.y);
    float halftone = step(halftonePattern, NdotL * (1.0 + halftoneStrength));

    half3 result = baseColor * (stepLight * 0.6 + 0.4);
    result = lerp(result, result * 1.3, halftone * halftoneStrength);
    return result;
}

// ---------------------------------------------------------------------------
// НОВАЯ ВЕРСИЯ: цветная тень, ambient, ступенчатые point/spot, rim, halftone в тенях
// ---------------------------------------------------------------------------
struct ArcaToonParams
{
    float  stepThreshold;
    float  stepSmooth;
    half3  shadowColor;       // множитель albedo в тени (не чёрный, а цветной)
    float  ambientInfluence;  // вклад ambient (SH) в тень
    float  addLightStep;      // порог ступеньки для point/spot
    half3  rimColor;
    float  rimThreshold;
    float  rimPower;
    float  halftoneScale;     // сколько ячеек по высоте экрана
    float  halftoneStrength;  // 0..1, яркость точек в тени
};

half3 ApplyArcaToonLightingEx(half3 albedo, float4 positionCS, float3 positionWS,
                              float3 normalWS, float4 shadowCoord, ArcaToonParams p)
{
    normalWS = normalize(normalWS);

    // --- главный свет ---
    Light mainLight = GetMainLight(shadowCoord);
    float NdotL = saturate(dot(normalWS, mainLight.direction));

    float litStep    = smoothstep(p.stepThreshold - p.stepSmooth, p.stepThreshold + p.stepSmooth, NdotL);
    // тень от сцены делаем жёсткой (даже при Soft Shadows), без размытого края
    float shadowStep = smoothstep(0.4, 0.6, mainLight.shadowAttenuation);
    float lit        = litStep * shadowStep;

    // --- цветная тень вместо затемнения ---
    half3 ambient   = SampleSH(normalWS) * p.ambientInfluence;
    half3 shadowCol = albedo * (p.shadowColor + ambient);
    half3 litCol    = albedo * mainLight.color;
    half3 col       = lerp(shadowCol, litCol, lit);

    // --- локальные источники: ступенчатые, цветные ---
    #if USE_FORWARD_PLUS || defined(_ADDITIONAL_LIGHTS)
        InputData inputData = (InputData)0;
        inputData.positionWS = positionWS;
        inputData.normalWS = normalWS;
        inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
        half4 shadowMask = half4(1, 1, 1, 1);

        uint pixelLightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light l = GetAdditionalLight(lightIndex, positionWS, shadowMask);
            float a = saturate(dot(normalWS, l.direction)) * l.distanceAttenuation * l.shadowAttenuation;
            float s = smoothstep(p.addLightStep - p.stepSmooth, p.addLightStep + p.stepSmooth, a);
            col += albedo * l.color * s;
        LIGHT_LOOP_END
    #endif

    // --- halftone только в тени: точки растут от границы тени вглубь ---
    {
        float cellPx = max(_ScreenParams.y / max(p.halftoneScale, 1.0), 2.0);
        float2 px = positionCS.xy;
        float2 rot = float2(px.x + px.y, px.x - px.y) * 0.70710678; // поворот 45°
        float2 cell = frac(rot / cellPx) - 0.5;
        float dotDist = length(cell);

        float depth = saturate(1.0 - NdotL / max(p.stepThreshold, 0.001));
        depth = max(depth, (1.0 - shadowStep) * 0.5);       // тень от объектов тоже штрихуется
        float dots = step(dotDist, depth * 0.75);

        float inShadow = 1.0 - lit;
        col = lerp(col, shadowCol * 0.65, dots * inShadow * p.halftoneStrength);
    }

    // --- rim: резкий контровой свет, только со стороны key ---
    half3 viewDir = GetWorldSpaceNormalizeViewDir(positionWS);
    float fres = pow(1.0 - saturate(dot(normalWS, viewDir)), p.rimPower);
    float rim = smoothstep(p.rimThreshold, p.rimThreshold + 0.02, fres) * saturate(NdotL + 0.3);
    col += rim * p.rimColor;

    return col;
}

#endif // ARCA_TOON_LIGHTING_INCLUDED

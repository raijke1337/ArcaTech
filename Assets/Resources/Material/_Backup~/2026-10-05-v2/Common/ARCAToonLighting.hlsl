#ifndef ARCA_TOON_LIGHTING_INCLUDED
#define ARCA_TOON_LIGHTING_INCLUDED

// Общий toon-степ + halftone-оверлей для непрозрачных ARCA toon-шейдеров.
// Требует, чтобы вызывающий шейдер подключил Lighting.hlsl и объявил pragma:
//   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
//   #pragma multi_compile _ _SHADOWS_SOFT
//   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
//   #pragma multi_compile _ _FORWARD_PLUS
//   #pragma multi_compile_fog
//
// Функции "кошачьего зрения" (ArcaVision*) требуют только Core.hlsl -
// их можно вызывать и из пассов обводки.

#include "ARCADither.hlsl" // GetBayer() для дизеринга границы зрения

// ===========================================================================
// КОШАЧЬЕ ЗРЕНИЕ - глобальные параметры (задаёт ArcaVisionController.cs)
// ===========================================================================
// Глобальные переменные объявлены ВНЕ CBUFFER UnityPerMaterial, поэтому
// SRP Batcher не ломается. Если на сцене нет контроллера, все значения = 0,
// _ArcaVisionParams.z = 0 -> зрение выключено, сцена рисуется как раньше.
float4 _ArcaVisionCenter;    // xyz - позиция Теилс, w - радиус полного зрения
float4 _ArcaVisionParams;    // x - ширина перехода, y - сила дизеринга, z - включено (0/1), w - яркость "ночи"
float4 _ArcaVisionParams2;   // x - сила дихромазии, y - доля эмиссии вне обзора, z - число ступеней перехода
half4  _ArcaNightTint;       // цвет "ночи" (множитель)
half4  _ArcaSilhouetteColor; // цвет обводки в темноте (кошка видит контуры)

// 1 - полностью видно (рядом с Теилс), 0 - за пределами обзора.
// Переход ступенчатый (toon) и сдизерен Bayer-матрицей - рисованная граница
// без "киберпространства": никаких колец, сеток и скан-линий.
float ArcaVisionFactor(float3 positionWS, float4 positionCS)
{
    if (_ArcaVisionParams.z < 0.5)
        return 1.0;

    // Дистанция по XZ: при изометрической камере высота не должна влиять
    // на то, что видно (лестницы, платформы, высокие стены).
    float dist = length(positionWS.xz - _ArcaVisionCenter.xz);
    float soft = max(_ArcaVisionParams.x, 0.001);
    // Внутри радиуса - 1, дальше спад до 0 на ширине перехода.
    float v    = saturate((_ArcaVisionCenter.w + soft - dist) / soft);

    // Ступени + упорядоченный дизеринг между ними.
    float bands  = max(_ArcaVisionParams2.z, 1.0);
    float bayer  = GetBayer(positionCS.xy);
    float jitter = lerp(0.5, bayer, saturate(_ArcaVisionParams.y));
    return saturate(floor(v * bands + jitter) / bands);
}

// Кошки - дихроматы: красный и зелёный сливаются, ось "синий-жёлтый" остаётся.
half3 ArcaCatDichromacy(half3 col)
{
    half lum    = dot(col, half3(0.299, 0.587, 0.114));
    half yellow = (col.r + col.g) * 0.5;
    half by     = col.b - yellow;                       // >0 синее, <0 жёлтое
    half3 d     = lum.xxx + by * half3(-0.3, -0.3, 0.6);
    return max(d, 0.0);
}

// Сначала уходит цвет, потом свет: на краю обзора мир становится
// сине-жёлтым и тусклым, дальше - тёмный силуэт в цвете _ArcaNightTint.
half3 ArcaApplyCatVision(half3 col, float vis)
{
    if (_ArcaVisionParams.z < 0.5)
        return col;

    half desat = saturate((1.0 - (half)vis) * 1.6);
    half dark  = 1.0 - (half)vis;

    half3 c     = lerp(col, ArcaCatDichromacy(col), desat * (half)_ArcaVisionParams2.x);
    half3 night = c * _ArcaNightTint.rgb * (half)_ArcaVisionParams.w;
    return lerp(c, night, dark);
}

// Эмиссия видна и в темноте (кошка замечает огни), но слабее.
half3 ArcaVisionEmission(half3 emission, float vis)
{
    if (_ArcaVisionParams.z < 0.5)
        return emission;
    return emission * lerp((half)_ArcaVisionParams2.y, 1.0, (half)vis);
}

// Обводка в темноте светлеет до _ArcaSilhouetteColor - читаются контуры.
half3 ArcaVisionOutline(half3 outlineColor, float vis)
{
    if (_ArcaVisionParams.z < 0.5)
        return outlineColor;
    return lerp(_ArcaSilhouetteColor.rgb, outlineColor, (half)vis);
}

// Освещение ниже требует Lighting.hlsl. Пассы без него (обводка, тени)
// получают только функции зрения выше.
#if defined(UNIVERSAL_LIGHTING_INCLUDED)

// ---------------------------------------------------------------------------
// СТАРАЯ ВЕРСИЯ - устарела, больше нигде не используется.
// Не учитывает цвет/силу света, локальные источники и туман.
// Оставлена на случай сторонних шейдеров; удалить после рефакторинга.
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
// ОСНОВНАЯ ВЕРСИЯ: цветная тень, ambient, ступенчатые point/spot, rim, halftone в тенях
// ---------------------------------------------------------------------------
struct ArcaToonParams
{
    float  stepThreshold;
    float  stepSmooth;
    half3  shadowColor;       // доля света в тени (цветная, не чёрная)
    float  ambientInfluence;  // вклад ambient (SH)
    float  addLightStep;      // порог первой ступеньки для point/spot
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

    // --- цветная тень ---
    // ИСПРАВЛЕНО: раньше тень была albedo * (shadowColor + ambient), т.е. не
    // зависела от силы света. При тусклом directional освещённая сторона
    // становилась ТЕМНЕЕ теневой. Теперь тень - это доля того же света,
    // а ambient одинаково подмешивается к обеим сторонам.
    half3 ambient   = SampleSH(normalWS) * p.ambientInfluence;
    half3 shadowCol = albedo * (mainLight.color * p.shadowColor + ambient);
    half3 litCol    = albedo * (mainLight.color + ambient);
    half3 col       = lerp(shadowCol, litCol, lit);

    // --- локальные источники: две ступени (ореол + ядро), цветные ---
    #if USE_FORWARD_PLUS || defined(_ADDITIONAL_LIGHTS)
        InputData inputData = (InputData)0;
        inputData.positionWS = positionWS;
        inputData.normalWS = normalWS;
        inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
        half4 shadowMask = half4(1, 1, 1, 1);

        float step2 = lerp(p.addLightStep, 1.0, 0.5);

        uint pixelLightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light l = GetAdditionalLight(lightIndex, positionWS, shadowMask);
            // Intensity лампы задаёт РАЗМЕР пятна (яркость падает как 1/d^2,
            // поэтому у сильной лампы ступенька срабатывает дальше), а цвет -
            // только оттенок. Яркость внутри пятна постоянная (toon).
            half  lum = max(dot(l.color, half3(0.299, 0.587, 0.114)), 0.0001);
            float a   = saturate(dot(normalWS, l.direction)) * l.distanceAttenuation * l.shadowAttenuation * lum;
            float s1  = smoothstep(p.addLightStep - p.stepSmooth, p.addLightStep + p.stepSmooth, a);
            float s2  = smoothstep(step2 - p.stepSmooth, step2 + p.stepSmooth, a);
            col += albedo * (l.color / lum) * (s1 * 0.5 + s2 * 0.5);
        LIGHT_LOOP_END
    #endif

    // --- halftone только в тени: точки растут от границы тени вглубь ---
    if (p.halftoneStrength > 0.001)
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
    // Теперь масштабируется цветом/силой главного света - в тёмной сцене
    // контуры не "горят" сами по себе.
    half3 viewDir = GetWorldSpaceNormalizeViewDir(positionWS);
    float fres = pow(1.0 - saturate(dot(normalWS, viewDir)), p.rimPower);
    float rim = smoothstep(p.rimThreshold, p.rimThreshold + 0.02, fres) * saturate(NdotL + 0.3);
    col += rim * p.rimColor * mainLight.color;

    return col;
}

#endif // UNIVERSAL_LIGHTING_INCLUDED

#endif // ARCA_TOON_LIGHTING_INCLUDED

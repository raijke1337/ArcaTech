#ifndef ARCA_TOON_LIGHTING_INCLUDED
#define ARCA_TOON_LIGHTING_INCLUDED

// Общий toon-степ + halftone-оверлей для непрозрачных ARCA toon-шейдеров
// (FadingWalls / стены, CoversShader / крышки и т.п.).
//
// В отличие от исходных версий, здесь учитывается затенение от других
// объектов сцены (mainLight.shadowAttenuation) — раньше GetMainLight()
// вызывался без shadowCoord, из-за чего тени от окружения полностью
// игнорировались toon-освещением.
//
// Требует, чтобы вызывающий шейдер подключил Lighting.hlsl и объявил
// #pragma multi_compile для теней главного света (см. использование ниже).
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

#endif // ARCA_TOON_LIGHTING_INCLUDED

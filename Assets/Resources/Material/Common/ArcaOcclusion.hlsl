#ifndef ARCA_OCCLUSION_INCLUDED
#define ARCA_OCCLUSION_INCLUDED

// ===========================================================================
// Прозрачность перекрывающей геометрии.
//
// Два независимых источника, итог - максимум:
//  1) _FadeAmount материала/MaterialPropertyBlock - растворение объекта
//     целиком (старый режим CameraTransparencyCaster, скрипты уровня);
//  2) вырез вокруг Теилс - дизеринговая "дыра" в трубке вдоль луча
//     камера -> Теилс. Работает попиксельно, без физики и без MPB,
//     поэтому большие стены и пол не исчезают целиком.
//
// Материал участвует в вырезе, если _ArcaOccluder = 1 (стены, укрытия).
// Пол не вырезается благодаря порогу высоты (_ArcaCutoutParams.y).
//
// Требует: Core.hlsl, ArcaGlobals.hlsl, ARCADither.hlsl.
// В CBUFFER шейдера должны быть: _FadeAmount, _ArcaOccluder.
// Для стен с локальным градиентом: #define ARCA_LOCALX_FADE и
// _FadeStartX, _FadeEndX (+ shader_feature _INVERTFADE_ON).
// ===========================================================================

// 0..1 - насколько пиксель попадает в вырез перед Теилс.
float ArcaCutoutFade(float3 positionWS)
{
    float radius = _ArcaCutoutRayDir.w;
    if (radius <= 0.0 || _ArcaCutoutParams.w <= 0.0)
        return 0.0;

    float3 d    = positionWS - _ArcaCutoutRayOrigin.xyz;
    float  t    = dot(d, _ArcaCutoutRayDir.xyz);           // расстояние вдоль луча
    float  r    = length(d - t * _ArcaCutoutRayDir.xyz);   // расстояние до луча
    float  soft = max(_ArcaCutoutParams.x, 0.001);

    float inTube  = 1.0 - smoothstep(radius - soft, radius, r);
    float inFront = step(t, _ArcaCutoutRayOrigin.w - _ArcaCutoutParams.z);
    float above   = smoothstep(_ArcaCutoutParams.y, _ArcaCutoutParams.y + soft, positionWS.y);

    return inTube * inFront * above * _ArcaCutoutParams.w;
}

// Прозрачность объекта целиком (_FadeAmount, локальный градиент стен).
float ArcaObjectFade(float localX)
{
#if defined(ARCA_LOCALX_FADE)
    float localFade = saturate((localX - _FadeStartX) / (_FadeEndX - _FadeStartX));
    #if defined(_INVERTFADE_ON)
        localFade = 1.0 - localFade;
    #endif
    // Глобальный фейд из скрипта приоритетнее локального градиента.
    return (_FadeAmount > 0.01) ? _FadeAmount : localFade;
#else
    return _FadeAmount;
#endif
}

// Итоговое отсечение пикселя.
// includeCutout = false для ShadowCaster: тени не должны пропадать из-за камеры.
void ArcaClipOcclusion(float4 positionCS, float3 positionWS, float localX, bool includeCutout)
{
    float fade = ArcaObjectFade(localX);
    if (includeCutout)
        fade = max(fade, ArcaCutoutFade(positionWS) * _ArcaOccluder);
    ArcaClipDither(positionCS, fade);
}

#endif // ARCA_OCCLUSION_INCLUDED

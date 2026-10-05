#ifndef ARCA_VISION_INCLUDED
#define ARCA_VISION_INCLUDED

// ===========================================================================
// "Кошачье зрение" Теилс.
// Рядом с Теилс - полный цвет. На краю обзора цвета уходят в сине-жёлтую
// гамму (кошки - дихроматы) и темнеют, дальше - тёмный силуэт, подсвеченная
// обводка и светящиеся объекты.
// Требует: Core.hlsl, ArcaGlobals.hlsl, ARCADither.hlsl.
// ===========================================================================

bool ArcaVisionEnabled()
{
    return _ArcaVisionCenter.w > 0.0;
}

// 1 - полностью видно, 0 - за пределами обзора.
// Переход ступенчатый (toon) и сдизерен Bayer-матрицей.
float ArcaVisionFactor(float3 positionWS, float4 positionCS)
{
    if (!ArcaVisionEnabled())
        return 1.0;

    // Дистанция по XZ: при изометрической камере высота не влияет на обзор.
    float dist = length(positionWS.xz - _ArcaVisionCenter.xz);
    float soft = max(_ArcaVisionStyle.x, 0.001);
    // Внутри радиуса - 1, дальше спад до 0 на ширине перехода.
    float v    = saturate((_ArcaVisionCenter.w + soft - dist) / soft);

    float bands  = max(_ArcaVisionStyle.z, 1.0);
    float bayer  = GetBayer(positionCS.xy);
    float jitter = lerp(0.5, bayer, saturate(_ArcaVisionStyle.y));
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

// Сначала уходит цвет, потом свет.
half3 ArcaApplyCatVision(half3 col, float vis)
{
    if (!ArcaVisionEnabled())
        return col;

    half desat = saturate((1.0 - (half)vis) * 1.6);
    half dark  = 1.0 - (half)vis;

    half3 c     = lerp(col, ArcaCatDichromacy(col), desat * (half)_ArcaVisionStyle2.x);
    half3 night = c * _ArcaNightTint.rgb * (half)_ArcaVisionStyle.w;
    return lerp(c, night, dark);
}

// Эмиссия видна и в темноте (кошка замечает огни), но слабее.
half3 ArcaVisionEmission(half3 emission, float vis)
{
    if (!ArcaVisionEnabled())
        return emission;
    return emission * lerp((half)_ArcaVisionStyle2.y, 1.0, (half)vis);
}

// Обводка в темноте светлеет до _ArcaSilhouetteColor - читаются контуры.
half3 ArcaVisionOutline(half3 outlineColor, float vis)
{
    if (!ArcaVisionEnabled())
        return outlineColor;
    return lerp(_ArcaSilhouetteColor.rgb, outlineColor, (half)vis);
}

#endif // ARCA_VISION_INCLUDED

#ifndef ARCA_OUTLINE_INCLUDED
#define ARCA_OUTLINE_INCLUDED

// ===========================================================================
// Обводка inverted hull с толщиной в ПИКСЕЛЯХ экрана.
// Раньше толщина задавалась в единицах объекта и зависела от масштаба меша
// (0.0001 у укрытий - не видно, 0.018 у стен). Теперь одинакова для всех.
//
// Источник нормали для выдавливания:
//  - обычная нормаль (на мешах с жёсткими гранями даёт разрывы на углах);
//  - сглаженная нормаль, запечённая в UV3 (TEXCOORD3) инструментом
//    SmoothNormalsBaker из Toon Pro (канал "UV3"). Включается
//    keyword _OUTLINE_SMOOTHNORMALS.
//
// Требует: Core.hlsl.
// ===========================================================================

float4 ArcaOutlinePositionCS(float3 positionOS, float3 outlineNormalOS, float widthPx)
{
    float4 positionCS = TransformObjectToHClip(positionOS);

    float3 normalWS = TransformObjectToWorldNormal(outlineNormalOS);
    float2 dirCS    = TransformWorldToHClipDir(normalWS).xy;
    float  len      = length(dirCS);
    dirCS = (len > 1e-5) ? dirCS / len : float2(0, 0);

    // пиксели -> NDC (x2, т.к. NDC от -1 до 1), домножаем на w для перспективы
    float2 screenSize = GetScaledScreenParams().xy;
    positionCS.xy += dirCS * (widthPx * 2.0 / screenSize) * positionCS.w;
    return positionCS;
}

#endif // ARCA_OUTLINE_INCLUDED

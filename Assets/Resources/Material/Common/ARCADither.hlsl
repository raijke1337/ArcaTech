#ifndef ARCA_DITHER_INCLUDED
#define ARCA_DITHER_INCLUDED

// Общая 4x4 Bayer-матрица для дизеринга прозрачности (AlphaToMask + clip).
//
// Раньше эта функция была продублирована в 6 местах по всем ARCA-шейдерам
// (FadingWalls x2 passes, CoversShader x2 passes, InactiveRoomShader) —
// любое изменение паттерна дизеринга требовалось вручную синхронизировать
// в шести копиях. Теперь один источник истины.
//
// screenPos - экранные координаты фрагмента (обычно input.positionCS.xy,
//             опционально домноженные на масштаб дизеринга)
// threshold - порог "прозрачности" (1.0 - finalFadeAmount), 0..1
// Возвращает значение, которое передаётся в clip(): >=0 - пиксель рисуется.
float GetDither(float2 screenPos, float threshold)
{
    int x = int(screenPos.x) % 4;
    int y = int(screenPos.y) % 4;
    int index = x + y * 4;

    float bayer[16] = {
        0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
        12.0 / 16.0, 4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
        3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
        15.0 / 16.0, 7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
    };

    return threshold - bayer[index];
}

// Удобный хелпер: считает итоговый fade (гибрид "глобальный из скрипта" /
// "локальный по оси X") и сразу клипует пиксель. Используется и в
// основном пассе, и в пассе обводки, и в ShadowCaster — раньше эта логика
// была раздельно скопирована в каждый пасс.
void ClipArcaFade(float4 positionCS, float localX, float fadeStartX, float fadeEndX,
                   float globalFadeAmount, bool invertLocalFade, float ditherScale)
{
    float localFade = saturate((localX - fadeStartX) / (fadeEndX - fadeStartX));
    if (invertLocalFade)
    {
        localFade = 1.0 - localFade;
    }

    // Если глобальный фейд (управляется из C#) активен - он имеет приоритет
    // над локальным градиентом по оси X.
    float finalFade = (globalFadeAmount > 0.01) ? globalFadeAmount : localFade;

    float2 ditherUV = positionCS.xy * ditherScale;
    float alphaThreshold = 1.0 - finalFade;
    clip(GetDither(ditherUV, alphaThreshold));
}

#endif // ARCA_DITHER_INCLUDED

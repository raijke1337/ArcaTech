#ifndef ARCA_GLOBALS_INCLUDED
#define ARCA_GLOBALS_INCLUDED

// ===========================================================================
// Глобальные параметры ARCA-шейдеров.
// Задаются из C# через Shader.SetGlobal* (см. Scripts/Rendering/ArcaShaderIds.cs).
// Объявлены ВНЕ CBUFFER UnityPerMaterial -> не мешают SRP Batcher.
// Если компонент, задающий группу параметров, отсутствует на сцене,
// значения равны 0 и соответствующий эффект выключен.
// ===========================================================================

// --- Кошачье зрение (ArcaVisionController + ArcaWorldLook) -----------------
float4 _ArcaVisionCenter;     // xyz - позиция Теилс, w - радиус полного зрения (<= 0 -> выключено)
float4 _ArcaVisionStyle;      // x - ширина перехода (м), y - сила дизеринга, z - число ступеней, w - яркость "ночи"
float4 _ArcaVisionStyle2;     // x - сила дихромазии, y - доля эмиссии в темноте
half4  _ArcaNightTint;        // оттенок мира за пределами обзора (множитель)
half4  _ArcaSilhouetteColor;  // цвет обводки в темноте

// --- Ambient биома (ArcaWorldLook) -----------------------------------------
// a у _ArcaAmbientSky = 1 -> используется градиент профиля, иначе SampleSH
// (Environment Lighting сцены).
half4  _ArcaAmbientSky;
half4  _ArcaAmbientEquator;
half4  _ArcaAmbientGround;

// --- Вырез перед Теилс (CameraTransparencyCaster) --------------------------
// Трубка вдоль луча "камера -> Теилс". Всё, что внутри трубки, ближе к камере,
// чем Теилс, и выше её колен, дизерингом становится прозрачным.
float4 _ArcaCutoutRayOrigin;  // xyz - начало луча, w - расстояние до Теилс вдоль луча
float4 _ArcaCutoutRayDir;     // xyz - направление луча (норм.), w - радиус трубки (<= 0 -> выключено)
float4 _ArcaCutoutParams;     // x - мягкость края (м), y - мин. высота мира, z - запас по глубине (м), w - сила 0..1

#endif // ARCA_GLOBALS_INCLUDED

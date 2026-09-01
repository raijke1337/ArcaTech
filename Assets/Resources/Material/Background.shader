Shader "Arcatech/Background/VoidScan"
{
    Properties
    {
        [Header(Base Colors)]
        _ColorDeep ("Deep Void Color", Color) = (0.02, 0.01, 0.05, 1)
        _ColorMid ("Mid Tone Color", Color) = (0.08, 0.04, 0.18, 1)
        _ColorHighlight ("Scan Highlight", Color) = (0.3, 0.6, 1.0, 1)
        
        [Header(Grid Settings)]
        _GridScale ("Grid Scale", Float) = 10.0
        _GridThickness ("Grid Thickness", Float) = 0.03
        _GridFadeDistance ("Grid Fade Distance", Float) = 50.0
        
        [Header(Scan Effect)]
        _ScanSpeed ("Scan Speed", Float) = 0.5
        _ScanWidth ("Scan Band Width", Float) = 2.0
        _ScanIntensity ("Scan Intensity", Float) = 0.8
        
        [Header(Noise Static)]
        _StaticSpeed ("Static Speed", Float) = 0.2
        _StaticIntensity ("Static Intensity", Float) = 0.15
        
        [Header(Meta Glitch Optional)]
        _GlitchChance ("Glitch Chance (0-1)", Range(0, 1)) = 0.05
        _TimeScale ("Global Time Scale", Float) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry-100" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }
        
        LOD 100
        ZWrite On
        Cull Back
        
        Pass
        {
            Name "VoidScanPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float depth : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _ColorDeep;
                half4 _ColorMid;
                half4 _ColorHighlight;
                float _GridScale;
                float _GridThickness;
                float _GridFadeDistance;
                float _ScanSpeed;
                float _ScanWidth;
                float _ScanIntensity;
                float _StaticSpeed;
                float _StaticIntensity;
                float _GlitchChance;
                float _TimeScale;
            CBUFFER_END
            
            // Простой хеш-шум для статики
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // Фрактальный шум для органичности
            float Noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f); // smoothstep
                
                float a = Hash(i);
                float b = Hash(i + float2(1.0, 0.0));
                float c = Hash(i + float2(0.0, 1.0));
                float d = Hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.depth = -TransformWorldToView(output.worldPos).z;
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _TimeScale;
                
                // === 1. Базовый градиент глубины ===
                float depthNorm = saturate(input.depth / _GridFadeDistance);
                half3 baseColor = lerp(_ColorMid.rgb, _ColorDeep.rgb, depthNorm);
                
                // === 2. Сетка (Grid) ===
                float2 gridUV = input.worldPos.xz / _GridScale;
                float2 gridDeriv = fwidth(gridUV);
                float2 gridAA = smoothstep(float2(0,0), gridDeriv * 1.5, abs(frac(gridUV - 0.5) - 0.5));
                float gridMask = 1.0 - min(gridAA.x, gridAA.y);
                // Затухание сетки с расстоянием
                gridMask *= saturate(1.0 - depthNorm * 1.2);
                
// === 3. Сканирующая полоса (Scan Band) - ИСПРАВЛЕНО ===
// Используем мировую координату Z (или X, в зависимости от ориентации плоскости)
// вместо UV, чтобы сканер шёл от края до края независимо от размера меша
float worldScanAxis = input.worldPos.z; 

// Нормализуем относительно масштаба сетки, чтобы скорость была одинаковой 
// на плоскостях разного размера
float scanWorldPos = worldScanAxis / _GridScale;

// Абсолютное время без frac() — полоса идёт бесконечно в одном направлении
// Модуль нужен только для предотвращения переполнения float при долгой игре
float scanTime = fmod(_Time.y * _ScanSpeed, 1000.0);

// Расстояние от текущей позиции сканера до каждой точки мира
float scanDist = abs(scanWorldPos - scanTime);

// Плавная полоса с чёткими краями
float scanBand = smoothstep(_ScanWidth, 0.0, scanDist);

// Опционально: затухание сканера на дальних краях плоскости
// (чтобы полоса не появлялась из ниоткуда на границе камеры)
float scanFade = saturate(1.0 - depthNorm * 0.8);
scanBand *= scanFade;
                
                // === 4. Статика / Шум ===
                float staticNoise = Noise(input.uv * 200.0 + time * _StaticSpeed);
                staticNoise = step(0.95, staticNoise) * _StaticIntensity;
                
                // === 5. Мета-глитч (опционально) ===
                float glitch = 0.0;
                if (_GlitchChance > 0.0)
                {
                    float glitchTrigger = step(1.0 - _GlitchChance, Hash(float2(time * 0.1, 0.0)));
                    float glitchLine = step(0.98, Hash(float2(input.uv.y * 50.0, time)));
                    glitch = glitchTrigger * glitchLine * 0.5;
                }
                
                // === Сборка финального цвета ===
                half3 finalColor = baseColor;
                finalColor += _ColorHighlight.rgb * gridMask * 0.4;
                finalColor += _ColorHighlight.rgb * scanBand * _ScanIntensity;
                finalColor += staticNoise;
                finalColor += glitch;
                
                // Легкая виньетка для фокусировки внимания к центру
                float vignette = 1.0 - length(input.uv - 0.5) * 0.8;
                finalColor *= saturate(vignette);
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
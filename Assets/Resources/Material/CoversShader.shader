Shader "ARCA/CoverToon"
{
    Properties
    {
        [Header(Base)]
        _BaseMap ("Diffuse (RGB)", 2D) = "white" {}
        _BaseColor ("Tint Color", Color) = (0.1, 0.13, 0.18, 1) // #1B2230
        
        [Header(Normal  Emission)]
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0.16, 0.84, 1.0, 1) // #29D7FF (Циан ARCA)
        _EmissionStrength ("Emission Strength", Float) = 1.0

        [Header(Toon  Halftone)]
        _StepThreshold ("Step Threshold", Range(0.0, 1.0)) = 0.5
        _HalftoneScale ("Halftone Scale", Float) = 80.0
        _HalftoneStrength ("Halftone Strength", Range(0.0, 1.0)) = 0.3

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.05, 0.05, 0.05, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.005

        [Header(Transparency)]
        _FadeAmount ("Fade Amount", Range(0.0, 1.0)) = 0.0 // Управляется из C#!
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }
        LOD 200

        // PASS 1: Основной рендер с Toon, Halftone и Dithering
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            AlphaToMask On // Важно для чистого Dithering в URP

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissionColor;
                float _BumpScale;
                float _EmissionStrength;
                float _StepThreshold;
                float _HalftoneScale;
                float _HalftoneStrength;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _FadeAmount;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 tangentOS : TANGENT; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1; float3 normalWS : TEXCOORD2; float3 tangentWS : TEXCOORD3; float3 bitangentWS : TEXCOORD4; };

            // 4x4 Bayer Matrix для Dithering
            float GetDither(float2 uv, float threshold)
            {
                int x = int(uv.x) % 4;
                int y = int(uv.y) % 4;
                int index = x + y * 4;
                float bayer[16] = { 0.0/16.0, 8.0/16.0, 2.0/16.0, 10.0/16.0,
                                    12.0/16.0, 4.0/16.0, 14.0/16.0, 6.0/16.0,
                                    3.0/16.0, 11.0/16.0, 1.0/16.0, 9.0/16.0,
                                    15.0/16.0, 7.0/16.0, 13.0/16.0, 5.0/16.0 };
                return threshold - bayer[index];
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Текстуры
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                half3 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb;

                // 2. Нормали в World Space
                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 normalWS = normalize(mul(normalTS, tangentToWorld));

                // 3. Освещение (Step Toon)
                Light mainLight = GetMainLight();
                float NdotL = max(0.0, dot(normalWS, mainLight.direction));
                float stepLight = smoothstep(_StepThreshold - 0.05, _StepThreshold + 0.05, NdotL); // Мягкий шаг
                
                // 4. Halftone оверлей (процедурный, реагирует на свет)
                float2 screenUV = input.positionCS.xy * _HalftoneScale / _ScreenParams.xy;
                float halftonePattern = sin(screenUV.x) * sin(screenUV.y);
                float halftone = step(halftonePattern, NdotL * (1.0 + _HalftoneStrength));
                
                // 5. Сборка цвета
                float3 finalColor = baseMap.rgb * _BaseColor.rgb * (stepLight * 0.6 + 0.4); // База + тень
                finalColor = lerp(finalColor, finalColor * 1.3, halftone * _HalftoneStrength); // Добавляем Halftone
                finalColor += emissionMap * _EmissionColor.rgb * _EmissionStrength; // Эмиссия

                // 6. Dithering прозрачность
                float2 ditherUV = input.positionCS.xy * 0.5; // Масштаб дизеринга
                float alphaThreshold = 1.0 - _FadeAmount;
                clip(GetDither(ditherUV, alphaThreshold));

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // PASS 2: Обводка (Inverted Hull)
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            AlphaToMask On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _FadeAmount;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            float GetDither(float2 uv, float threshold) // Дублируем для пассов
            {
                int x = int(uv.x) % 4; int y = int(uv.y) % 4; int index = x + y * 4;
                float bayer[16] = { 0.0/16.0, 8.0/16.0, 2.0/16.0, 10.0/16.0, 12.0/16.0, 4.0/16.0, 14.0/16.0, 6.0/16.0, 3.0/16.0, 11.0/16.0, 1.0/16.0, 9.0/16.0, 15.0/16.0, 7.0/16.0, 13.0/16.0, 5.0/16.0 };
                return threshold - bayer[index];
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Смещение по нормали для обводки
                float3 positionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth * 100.0; 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 ditherUV = input.positionCS.xy * 0.5;
                float alphaThreshold = 1.0 - _FadeAmount;
                clip(GetDither(ditherUV, alphaThreshold));
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
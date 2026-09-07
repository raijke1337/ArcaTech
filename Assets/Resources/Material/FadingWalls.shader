Shader "ARCA/WallFade"
{
    Properties
    {
        [Header(Base)]
        _BaseColor ("Base Color", Color) = (0.1, 0.13, 0.18, 1) // #1B2230
        
        [Header(Overlay Texture)]
        _OverlayMap ("Overlay Texture (RGB)", 2D) = "white" {}
        _OverlayScale ("Overlay Scale (World Units)", Float) = 1.0
        _OverlayStrength ("Overlay Strength", Range(0.0, 1.0)) = 1.0
        
        [Header(Toon  Halftone)]
        _StepThreshold ("Step Threshold", Range(0.0, 1.0)) = 0.5
        _HalftoneScale ("Halftone Scale", Float) = 80.0
        _HalftoneStrength ("Halftone Strength", Range(0.0, 1.0)) = 0.3

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.05, 0.05, 0.05, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.01 

        [Header(Fade Control)]
        // Новое свойство для управления из C#
        _FadeAmount ("Global Fade Amount (Script Controlled)", Range(0.0, 1.0)) = 0.0
        
        // Старые свойства для локального градиента (остаются для совместимости)
        _FadeStartX ("Fade Start (Local X)", Float) = 0.0
        _FadeEndX ("Fade End (Local X)", Float) = -1.0
        [Toggle(_INVERTFADE_ON)] _InvertFade ("Invert Fade Direction", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }
        LOD 200

        // PASS 1: Main Render
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            AlphaToMask On 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _INVERTFADE_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _OverlayMap_ST;
                float _OverlayScale;
                float _OverlayStrength;
                float _StepThreshold;
                float _HalftoneScale;
                float _HalftoneStrength;
                float4 _OutlineColor;
                float _OutlineWidth;
                
                // Параметры фейда
                float _FadeAmount;      // Глобальный (из скрипта)
                float _FadeStartX;      // Локальный старт
                float _FadeEndX;        // Локальный конец
            CBUFFER_END

            TEXTURE2D(_OverlayMap); SAMPLER(sampler_OverlayMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            
            struct Varyings 
            { 
                float4 positionCS : SV_POSITION; 
                float3 positionWS : TEXCOORD0; 
                float3 normalWS : TEXCOORD1; 
                float localX : TEXCOORD2; 
            };

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
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.localX = input.positionOS.x;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Базовый цвет и оверлей (Triplanar)
                half3 finalColor = _BaseColor.rgb;

                float3 blendWeights = abs(input.normalWS);
                blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

                float2 uvX = input.positionWS.zy * _OverlayScale;
                float2 uvY = input.positionWS.xz * _OverlayScale;
                float2 uvZ = input.positionWS.xy * _OverlayScale;

                half4 texX = SAMPLE_TEXTURE2D(_OverlayMap, sampler_OverlayMap, uvX);
                half4 texY = SAMPLE_TEXTURE2D(_OverlayMap, sampler_OverlayMap, uvY);
                half4 texZ = SAMPLE_TEXTURE2D(_OverlayMap, sampler_OverlayMap, uvZ);

                half4 overlayTex = texX * blendWeights.x + texY * blendWeights.y + texZ * blendWeights.z;
                float luminance = dot(overlayTex.rgb, float3(0.299, 0.587, 0.114));
                float mask = 1.0 - luminance; 
                
                finalColor = lerp(finalColor, overlayTex.rgb, mask * _OverlayStrength);

                // 2. Освещение и Halftone
                Light mainLight = GetMainLight();
                float NdotL = max(0.0, dot(input.normalWS, mainLight.direction));
                float stepLight = smoothstep(_StepThreshold - 0.05, _StepThreshold + 0.05, NdotL);
                
                float2 screenUV = input.positionCS.xy * _HalftoneScale / _ScreenParams.xy;
                float halftonePattern = sin(screenUV.x) * sin(screenUV.y);
                float halftone = step(halftonePattern, NdotL * (1.0 + _HalftoneStrength));
                
                finalColor = finalColor * (stepLight * 0.6 + 0.4);
                finalColor = lerp(finalColor, finalColor * 1.3, halftone * _HalftoneStrength);

                // 3. Расчет прозрачности (Гибридный режим)
                
                // А. Локальный расчет (по оси X)
                float localFade = saturate((input.localX - _FadeStartX) / (_FadeEndX - _FadeStartX));
                #if defined(_INVERTFADE_ON)
                    localFade = 1.0 - localFade;
                #endif

                // Б. Глобальный расчет (из скрипта)
                float globalFade = _FadeAmount;

                // В. Итоговое значение: Если глобальный фейд активен (> 0.01), он имеет приоритет.
                // Иначе используется локальный градиент.
                float finalFade = (globalFade > 0.01) ? globalFade : localFade;

                // 4. Dithering
                float2 ditherUV = input.positionCS.xy * 0.5;
                float alphaThreshold = 1.0 - finalFade;
                clip(GetDither(ditherUV, alphaThreshold));

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // PASS 2: Outline
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            AlphaToMask On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _INVERTFADE_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _FadeAmount;
                float _FadeStartX;
                float _FadeEndX;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float localX : TEXCOORD0; };

            float GetDither(float2 uv, float threshold)
            {
                int x = int(uv.x) % 4; int y = int(uv.y) % 4; int index = x + y * 4;
                float bayer[16] = { 0.0/16.0, 8.0/16.0, 2.0/16.0, 10.0/16.0, 12.0/16.0, 4.0/16.0, 14.0/16.0, 6.0/16.0, 3.0/16.0, 11.0/16.0, 1.0/16.0, 9.0/16.0, 15.0/16.0, 7.0/16.0, 13.0/16.0, 5.0/16.0 };
                return threshold - bayer[index];
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth; 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                output.positionCS = vertexInput.positionCS;
                output.localX = input.positionOS.x;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Та же логика гибридного фейда для обводки
                float localFade = saturate((input.localX - _FadeStartX) / (_FadeEndX - _FadeStartX));
                #if defined(_INVERTFADE_ON)
                    localFade = 1.0 - localFade;
                #endif
                
                float finalFade = (_FadeAmount > 0.01) ? _FadeAmount : localFade;

                float2 ditherUV = input.positionCS.xy * 0.5;
                float alphaThreshold = 1.0 - finalFade;
                clip(GetDither(ditherUV, alphaThreshold));
                
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
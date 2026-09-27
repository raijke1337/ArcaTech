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

            // Тени главного света (раньше GetMainLight() вызывался без
            // shadowCoord и тени от сцены полностью игнорировались).
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Common/ARCADither.hlsl"
            #include "Common/ARCAToonLighting.hlsl"

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
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
            };

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
                output.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);
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

                // 3-4. Toon-освещение + Halftone (с учётом теней от сцены)
                half3 finalColor = ApplyArcaToonLighting(baseMap.rgb * _BaseColor.rgb, input.positionCS,
                                                          normalWS, input.shadowCoord, _StepThreshold,
                                                          _HalftoneScale, _HalftoneStrength);

                // 5. Эмиссия
                finalColor += emissionMap * _EmissionColor.rgb * _EmissionStrength;

                // 6. Dithering прозрачность
                float2 ditherUV = input.positionCS.xy * 0.5;
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
            #include "Common/ARCADither.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _FadeAmount;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                // ИСПРАВЛЕНО: было "* _OutlineWidth * 100.0" - при одинаковом
                // значении слайдера (0..0.05) обводка на Covers оказывалась
                // в 100 раз толще, чем на стенах (FadingWalls использует
                // "* _OutlineWidth" без множителя). Теперь масштаб одинаков
                // для всех ARCA toon-шейдеров, и художник может настраивать
                // единый параметр в инспекторе без сюрпризов.
                float3 positionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;
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

        // PASS 3: ShadowCaster
        // Раньше отсутствовал, как и у FadingWalls: без него объект либо
        // не отбрасывал тень, либо использовал дефолтный Fallback, который
        // не знает про _FadeAmount/дизеринг.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Common/ARCADither.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _FadeAmount;
            CBUFFER_END

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                positionWS = ApplyShadowBias(positionWS, normalWS, lightDirectionWS);
                output.positionCS = TransformWorldToHClip(positionWS);

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                float2 ditherUV = input.positionCS.xy * 0.5;
                float alphaThreshold = 1.0 - _FadeAmount;
                clip(GetDither(ditherUV, alphaThreshold));
                return 0;
            }
            ENDHLSL
        }
    }
}

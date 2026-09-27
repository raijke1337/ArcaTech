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

            // Тени главного света: без этого GetMainLight(shadowCoord) всегда
            // возвращал бы shadowAttenuation = 1, и стены игнорировали бы тени
            // от других объектов сцены.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Common/ARCADither.hlsl"
            #include "Common/ARCAToonLighting.hlsl"

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
                float4 shadowCoord : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.localX = input.positionOS.x;
                output.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);
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

                // 2. Освещение и Halftone (теперь с учётом теней от сцены)
                finalColor = ApplyArcaToonLighting(finalColor, input.positionCS, input.normalWS,
                                                    input.shadowCoord, _StepThreshold,
                                                    _HalftoneScale, _HalftoneStrength);

                // 3. Прозрачность и дизеринг (гибридный режим: глобальный fade
                //    из скрипта приоритетнее локального градиента по X)
                #if defined(_INVERTFADE_ON)
                    bool invertFade = true;
                #else
                    bool invertFade = false;
                #endif
                ClipArcaFade(input.positionCS, input.localX, _FadeStartX, _FadeEndX,
                             _FadeAmount, invertFade, 0.5);

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
            #include "Common/ARCADither.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _FadeAmount;
                float _FadeStartX;
                float _FadeEndX;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float localX : TEXCOORD0; };

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
                #if defined(_INVERTFADE_ON)
                    bool invertFade = true;
                #else
                    bool invertFade = false;
                #endif
                ClipArcaFade(input.positionCS, input.localX, _FadeStartX, _FadeEndX,
                             _FadeAmount, invertFade, 0.5);

                return _OutlineColor;
            }
            ENDHLSL
        }

        // PASS 3: ShadowCaster
        // Раньше отсутствовал: стены либо не отбрасывали тени, либо
        // URP подставлял дефолтный ShadowCaster, который НЕ знает про
        // _FadeAmount/дизеринг - зафейженная в 0 (визуально невидимая)
        // стена всё равно отбрасывала бы полную тень. Здесь тень клипуется
        // той же дизеринг-маской, что и основной цвет.
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
            #pragma shader_feature_local _INVERTFADE_ON
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Common/ARCADither.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _FadeAmount;
                float _FadeStartX;
                float _FadeEndX;
            CBUFFER_END

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float localX : TEXCOORD0; };

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
                output.localX = input.positionOS.x;

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                #if defined(_INVERTFADE_ON)
                    bool invertFade = true;
                #else
                    bool invertFade = false;
                #endif
                ClipArcaFade(input.positionCS, input.localX, _FadeStartX, _FadeEndX,
                             _FadeAmount, invertFade, 0.5);

                return 0;
            }
            ENDHLSL
        }
    }
}

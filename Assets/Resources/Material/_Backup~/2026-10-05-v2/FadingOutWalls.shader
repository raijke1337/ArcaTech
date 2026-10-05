Shader "ARCA/WallFade"
{
    // 2026-10-05: переведён на общее ядро ApplyArcaToonLightingEx.
    //  - стены реагируют на цвет/силу главного света, point/spot и туман
    //    (раньше яркость была зашита: base * (0.4..1.0) при любом свете);
    //  - "кошачье зрение": за радиусом обзора Теилс - тёмный силуэт;
    //  - один CBUFFER во всех пассах -> совместимость с SRP Batcher.
    // Оригинал: _Backup~/2026-10-05/FadingOutWalls.shader
    Properties
    {
        [Header(Base)]
        _BaseColor ("Base Color", Color) = (0.1, 0.13, 0.18, 1) // #1B2230

        [Header(Overlay Texture)]
        _OverlayMap ("Overlay Texture (RGB)", 2D) = "white" {}
        _OverlayScale ("Overlay Scale (World Units)", Float) = 1.0
        _OverlayStrength ("Overlay Strength", Range(0.0, 1.0)) = 1.0

        [Header(Toon  Shadow)]
        _StepThreshold ("Step Threshold", Range(0.0, 1.0)) = 0.5
        _StepSmooth ("Step Smoothness", Range(0.001, 0.1)) = 0.02
        _ShadowColor ("Shadow Tint (доля света в тени)", Color) = (0.35, 0.33, 0.5, 1)
        _AmbientInfluence ("Ambient Influence", Range(0.0, 1.0)) = 0.5
        _AddLightStep ("Point Light Step", Range(0.0, 1.0)) = 0.25

        [Header(Halftone in Shadow)]
        _HalftoneScale ("Halftone Cells (per screen height)", Float) = 80.0
        _HalftoneStrength ("Halftone Strength", Range(0.0, 1.0)) = 0.0

        [Header(Rim)]
        _RimColor ("Rim Color", Color) = (0, 0, 0, 1)
        _RimThreshold ("Rim Threshold", Range(0.0, 1.0)) = 0.6
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.05, 0.05, 0.05, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.01

        [Header(Fade Control)]
        _FadeAmount ("Global Fade Amount (Script Controlled)", Range(0.0, 1.0)) = 0.0
        _FadeStartX ("Fade Start (Local X)", Float) = 0.0
        _FadeEndX ("Fade End (Local X)", Float) = -1.0
        [Toggle(_INVERTFADE_ON)] _InvertFade ("Invert Fade Direction", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }
        LOD 200

        // Один и тот же CBUFFER во всех пассах - обязательное условие SRP Batcher.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _OverlayMap_ST;
            float  _OverlayScale;
            float  _OverlayStrength;
            float  _StepThreshold;
            float  _StepSmooth;
            float4 _ShadowColor;
            float  _AmbientInfluence;
            float  _AddLightStep;
            float  _HalftoneScale;
            float  _HalftoneStrength;
            float4 _RimColor;
            float  _RimThreshold;
            float  _RimPower;
            float4 _OutlineColor;
            float  _OutlineWidth;
            float  _FadeAmount;      // Глобальный (из скрипта)
            float  _FadeStartX;      // Локальный старт
            float  _FadeEndX;        // Локальный конец
        CBUFFER_END
        ENDHLSL

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

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Common/ARCADither.hlsl"
            #include "Common/ARCAToonLighting.hlsl"

            TEXTURE2D(_OverlayMap); SAMPLER(sampler_OverlayMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float localX : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                half fogFactor : TEXCOORD4;
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
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 0. Прозрачность и дизеринг - первым делом, чтобы не считать
                //    освещение для отброшенных пикселей.
                #if defined(_INVERTFADE_ON)
                    bool invertFade = true;
                #else
                    bool invertFade = false;
                #endif
                ClipArcaFade(input.positionCS, input.localX, _FadeStartX, _FadeEndX,
                             _FadeAmount, invertFade, 0.5);

                float3 normalWS = normalize(input.normalWS);

                // 1. Базовый цвет и оверлей (Triplanar)
                half3 albedo = _BaseColor.rgb;

                float3 blendWeights = abs(normalWS);
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

                albedo = lerp(albedo, overlayTex.rgb, mask * _OverlayStrength);

                // 2. Toon-освещение (общее ядро с CoversShader)
                ArcaToonParams tp;
                tp.stepThreshold    = _StepThreshold;
                tp.stepSmooth       = _StepSmooth;
                tp.shadowColor      = _ShadowColor.rgb;
                tp.ambientInfluence = _AmbientInfluence;
                tp.addLightStep     = _AddLightStep;
                tp.rimColor         = _RimColor.rgb;
                tp.rimThreshold     = _RimThreshold;
                tp.rimPower         = _RimPower;
                tp.halftoneScale    = _HalftoneScale;
                tp.halftoneStrength = _HalftoneStrength;

                half3 finalColor = ApplyArcaToonLightingEx(albedo, input.positionCS, input.positionWS,
                                                           normalWS, input.shadowCoord, tp);

                // 3. Кошачье зрение
                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                finalColor = ArcaApplyCatVision(finalColor, vis);

                // 4. Туман
                finalColor = MixFog(finalColor, input.fogFactor);

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
            #pragma multi_compile_fog
            #include "Common/ARCADither.hlsl"
            #include "Common/ARCAToonLighting.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float localX : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.localX = input.positionOS.x;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
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

                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                half3 col = ArcaVisionOutline(_OutlineColor.rgb, vis);
                col = MixFog(col, input.fogFactor);
                return half4(col, 1.0);
            }
            ENDHLSL
        }

        // PASS 3: ShadowCaster (клипуется той же маской, что и цвет)
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Common/ARCADither.hlsl"

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

        // PASS 4: DepthOnly - нужен для depth prepass (SSAO, эффекты по глубине).
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma shader_feature_local _INVERTFADE_ON
            #include "Common/ARCADither.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float localX : TEXCOORD0; };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.localX = input.positionOS.x;
                return output;
            }

            half DepthFrag(Varyings input) : SV_TARGET
            {
                #if defined(_INVERTFADE_ON)
                    bool invertFade = true;
                #else
                    bool invertFade = false;
                #endif
                ClipArcaFade(input.positionCS, input.localX, _FadeStartX, _FadeEndX,
                             _FadeAmount, invertFade, 0.5);
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
}

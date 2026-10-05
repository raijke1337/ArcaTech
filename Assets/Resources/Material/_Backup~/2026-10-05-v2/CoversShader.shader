Shader "ARCA/CoverToon"
{
    // 2026-10-05: "кошачье зрение" (ArcaVisionFactor), обводка-силуэт в темноте,
    // DepthOnly-пасс. Исправленная формула тени - в Common/ARCAToonLighting.hlsl.
    // Оригинал: _Backup~/2026-10-05/CoversShader.shader
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

        [Header(Toon  Shadow)]
        _StepThreshold ("Step Threshold", Range(0.0, 1.0)) = 0.5
        _StepSmooth ("Step Smoothness", Range(0.001, 0.1)) = 0.02
        _ShadowColor ("Shadow Tint", Color) = (0.28, 0.22, 0.5, 1)
        _AmbientInfluence ("Ambient Influence", Range(0.0, 1.0)) = 0.5
        _AddLightStep ("Point Light Step", Range(0.0, 1.0)) = 0.35

        [Header(Halftone in Shadow)]
        _HalftoneScale ("Halftone Cells (per screen height)", Float) = 80.0
        _HalftoneStrength ("Halftone Strength", Range(0.0, 1.0)) = 0.6

        [Header(Rim)]
        _RimColor ("Rim Color", Color) = (0.16, 0.84, 1.0, 1)
        _RimThreshold ("Rim Threshold", Range(0.0, 1.0)) = 0.6
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0

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

        // Один и тот же CBUFFER во всех пассах - обязательное условие SRP Batcher.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _EmissionColor;
            float _BumpScale;
            float _EmissionStrength;
            float _StepThreshold;
            float _StepSmooth;
            float4 _ShadowColor;
            float _AmbientInfluence;
            float _AddLightStep;
            float _HalftoneScale;
            float _HalftoneStrength;
            float4 _RimColor;
            float _RimThreshold;
            float _RimPower;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _FadeAmount;
        CBUFFER_END
        ENDHLSL

        // PASS 1: Основной рендер с Toon, цветной тенью, Halftone и Dithering
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            AlphaToMask On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Common/ARCADither.hlsl"
            #include "Common/ARCAToonLighting.hlsl"

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
                half fogFactor : TEXCOORD6;
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
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
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

                // 3. Toon-освещение: цветная тень, point/spot, halftone в тени, rim
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

                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                half3 finalColor = ApplyArcaToonLightingEx(albedo, input.positionCS, input.positionWS,
                                                           normalWS, input.shadowCoord, tp);

                // 4. Кошачье зрение: вдали от Теилс - дихромазия и темнота
                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                finalColor = ArcaApplyCatVision(finalColor, vis);

                // 5. Эмиссия (огни видны и в темноте, но слабее)
                finalColor += ArcaVisionEmission(emissionMap * _EmissionColor.rgb * _EmissionStrength, vis);

                // 6. Туман
                finalColor = MixFog(finalColor, input.fogFactor);

                // 7. Dithering прозрачность
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
            #pragma multi_compile_fog
            #include "Common/ARCADither.hlsl"
            #include "Common/ARCAToonLighting.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 ditherUV = input.positionCS.xy * 0.5;
                float alphaThreshold = 1.0 - _FadeAmount;
                clip(GetDither(ditherUV, alphaThreshold));

                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                half3 col = ArcaVisionOutline(_OutlineColor.rgb, vis);
                col = MixFog(col, input.fogFactor);
                return half4(col, 1.0);
            }
            ENDHLSL
        }

        // PASS 3: ShadowCaster
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Common/ARCADither.hlsl"

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
            #include "Common/ARCADither.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half DepthFrag(Varyings input) : SV_TARGET
            {
                clip(GetDither(input.positionCS.xy * 0.5, 1.0 - _FadeAmount));
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
}

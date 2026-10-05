Shader "ARCA/WallFade"
{
    // Стены уровня: triplanar-оверлей + общее ARCA-освещение.
    // Прозрачность: _FadeAmount (скрипт) / локальный градиент по X /
    // вырез перед Теилс (CameraTransparencyCaster).
    // Предыдущие версии: _Backup~/
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
        _OutlineWidthPx ("Outline Width (px)", Range(0.0, 6.0)) = 1.5

        [Header(Fade Control)]
        _FadeAmount ("Global Fade Amount (Script Controlled)", Range(0.0, 1.0)) = 0.0
        _FadeStartX ("Fade Start (Local X)", Float) = 0.0
        _FadeEndX ("Fade End (Local X)", Float) = -1.0
        [Toggle(_INVERTFADE_ON)] _InvertFade ("Invert Fade Direction", Float) = 0
        [Toggle] _ArcaOccluder ("Вырезать перед Теилс", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }
        LOD 200

        // Один и тот же CBUFFER во всех пассах - условие SRP Batcher.
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
            float  _OutlineWidthPx;
            float  _FadeAmount;
            float  _FadeStartX;
            float  _FadeEndX;
            float  _ArcaOccluder;
        CBUFFER_END

        // Стены используют локальный градиент прозрачности по оси X.
        #define ARCA_LOCALX_FADE 1

        #include "Common/ArcaGlobals.hlsl"
        #include "Common/ARCADither.hlsl"
        #include "Common/ArcaVision.hlsl"
        #include "Common/ArcaOcclusion.hlsl"
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
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Common/ArcaLighting.hlsl"

            TEXTURE2D(_OverlayMap); SAMPLER(sampler_OverlayMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float  localX     : TEXCOORD2;
                half   fogFactor  : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS   = normalInput.normalWS;
                output.localX     = input.positionOS.x;
                output.fogFactor  = ComputeFogFactor(vertexInput.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 0. Прозрачность - первой
                ArcaClipOcclusion(input.positionCS, input.positionWS, input.localX, true);

                float3 normalWS = normalize(input.normalWS);

                // 1. Базовый цвет и оверлей (triplanar)
                float3 w = abs(normalWS);
                w /= (w.x + w.y + w.z);

                half4 texX = SAMPLE_TEXTURE2D(_OverlayMap, sampler_OverlayMap, input.positionWS.zy * _OverlayScale);
                half4 texY = SAMPLE_TEXTURE2D(_OverlayMap, sampler_OverlayMap, input.positionWS.xz * _OverlayScale);
                half4 texZ = SAMPLE_TEXTURE2D(_OverlayMap, sampler_OverlayMap, input.positionWS.xy * _OverlayScale);
                half4 overlayTex = texX * w.x + texY * w.y + texZ * w.z;

                float mask = 1.0 - dot(overlayTex.rgb, float3(0.299, 0.587, 0.114));
                half3 albedo = lerp(_BaseColor.rgb, overlayTex.rgb, mask * _OverlayStrength);

                // 2. Toon-освещение (общее ядро)
                ArcaSurface s;
                s.albedo     = albedo;
                s.occlusion  = 1.0;
                s.positionWS = input.positionWS;
                s.normalWS   = normalWS;
                s.positionCS = input.positionCS;

                ArcaToonParams tp   = ArcaDefaultToonParams();
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

                half3 color = ArcaToonLighting(s, tp);

                // 3. Кошачье зрение
                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                color = ArcaApplyCatVision(color, vis);

                // 4. Туман
                return half4(MixFog(color, input.fogFactor), 1.0);
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
            #include "Common/ArcaOutline.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float  localX     : TEXCOORD1;
                half   fogFactor  : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = ArcaOutlinePositionCS(input.positionOS.xyz, input.normalOS, _OutlineWidthPx);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.localX     = input.positionOS.x;
                output.fogFactor  = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                ArcaClipOcclusion(input.positionCS, input.positionWS, input.localX, true);
                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                half3 col = ArcaVisionOutline(_OutlineColor.rgb, vis);
                return half4(MixFog(col, input.fogFactor), 1.0);
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
            #pragma vertex ArcaShadowVert
            #pragma fragment ArcaShadowFrag
            #pragma shader_feature_local _INVERTFADE_ON
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Common/ArcaPasses.hlsl"
            ENDHLSL
        }

        // PASS 4: DepthOnly
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma vertex ArcaDepthVert
            #pragma fragment ArcaDepthFrag
            #pragma shader_feature_local _INVERTFADE_ON
            #include "Common/ArcaPasses.hlsl"
            ENDHLSL
        }

        // PASS 5: DepthNormals (SSAO)
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex ArcaDepthVert
            #pragma fragment ArcaDepthNormalsFrag
            #pragma shader_feature_local _INVERTFADE_ON
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include "Common/ArcaPasses.hlsl"
            ENDHLSL
        }
    }
}

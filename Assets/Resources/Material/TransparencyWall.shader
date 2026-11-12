Shader "ARCA/CoverToon"
{
    // Твёрдые объекты окружения (укрытия, ящики, пол).
    // Общие модули: Common/Arca*.hlsl. Освещение, зрение и прозрачность
    // одинаковы со стенами и персонажами.
    // Предыдущие версии: _Backup~/
    Properties
    {
        [Header(Base)]
        _BaseMap ("Diffuse (RGB)", 2D) = "white" {}
        _BaseColor ("Tint Color", Color) = (0.1, 0.13, 0.18, 1) // #1B2230

        [Header(Normal  Emission)]
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _EmissionMap ("Emission Map", 2D) = "black" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0.16, 0.84, 1.0, 1) // #29D7FF (Циан ARCA)
        _EmissionStrength ("Emission Strength", Float) = 1.0

        [Header(Toon  Shadow)]
        _StepThreshold ("Step Threshold", Range(0.0, 1.0)) = 0.5
        _StepSmooth ("Step Smoothness", Range(0.001, 0.1)) = 0.02
        _ShadowColor ("Shadow Tint (доля света в тени)", Color) = (0.28, 0.22, 0.5, 1)
        _AmbientInfluence ("Ambient Influence", Range(0.0, 1.0)) = 0.5
        _AddLightStep ("Point Light Step", Range(0.0, 1.0)) = 0.3

        [Header(Halftone in Shadow)]
        _HalftoneScale ("Halftone Cells (per screen height)", Float) = 80.0
        _HalftoneStrength ("Halftone Strength", Range(0.0, 1.0)) = 0.0

        [Header(Rim)]
        _RimColor ("Rim Color", Color) = (0.16, 0.84, 1.0, 1)
        _RimThreshold ("Rim Threshold", Range(0.0, 1.0)) = 0.6
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.05, 0.05, 0.05, 1)
        _OutlineWidthPx ("Outline Width (px)", Range(0.0, 6.0)) = 1.0

        [Header(Transparency)]
        _FadeAmount ("Fade Amount (скрипт)", Range(0.0, 1.0)) = 0.0
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
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _EmissionColor;
            float  _BumpScale;
            float  _EmissionStrength;
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
            float  _ArcaOccluder;
        CBUFFER_END

        #include "Common/ArcaGlobals.hlsl"
        #include "Common/ARCADither.hlsl"
        #include "Common/ArcaVision.hlsl"
        #include "Common/ArcaOcclusion.hlsl"
        ENDHLSL

        // PASS 1: Основной рендер
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            AlphaToMask On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Common/ArcaLighting.hlsl"

            TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);     SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                half   fogFactor   : TEXCOORD5;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS  = vertexInput.positionCS;
                output.uv          = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionWS  = vertexInput.positionWS;
                output.normalWS    = normalInput.normalWS;
                output.tangentWS   = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.fogFactor   = ComputeFogFactor(vertexInput.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 0. Прозрачность - первой, чтобы не считать свет зря
                ArcaClipOcclusion(input.positionCS, input.positionWS, 0.0, true);

                // 1. Текстуры
                half4 baseMap  = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb
                                 * _EmissionColor.rgb * _EmissionStrength;

                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);

                // 2. Toon-освещение
                ArcaSurface s;
                s.albedo     = baseMap.rgb * _BaseColor.rgb;
                s.occlusion  = 1.0;
                s.positionWS = input.positionWS;
                s.normalWS   = normalize(mul(normalTS, tangentToWorld));
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

                // 3. Кошачье зрение + эмиссия (огни видны и в темноте)
                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                color  = ArcaApplyCatVision(color, vis);
                color += ArcaVisionEmission(emission, vis);

                // 4. Туман
                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // PASS 2: Обводка (inverted hull, толщина в пикселях)
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
            #include "Common/ArcaOutline.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half   fogFactor  : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = ArcaOutlinePositionCS(input.positionOS.xyz, input.normalOS, _OutlineWidthPx);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.fogFactor  = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                ArcaClipOcclusion(input.positionCS, input.positionWS, 0.0, true);
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
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include "Common/ArcaPasses.hlsl"
            ENDHLSL
        }
    }
}

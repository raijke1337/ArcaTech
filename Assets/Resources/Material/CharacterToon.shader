Shader "ARCA/CharacterToon"
{
    // Персонажи и металлы: замена Toon Pro на общем ядре ARCA.
    // По умолчанию не отбрасывает тень от фонаря Теилс (галка в Shadows):
    // ни Теилс, ни враги не дают теней от лампы над героиней.
    // Реагирует на свет так же, как окружение: Intensity point-света задаёт
    // размер пятна, а не яркость (больше никакого "светящегося" персонажа).
    //
    // Металл (_METAL): ступенчатый блик + matcap.
    // Без шейдинга (_UNSHADED): плоский цвет альбедо без света и теней;
    // matcap, эмиссия, обводка, кошачье зрение и прозрачность сохраняются.
    // Обводка: толщина в пикселях; для мягкого контура без разрывов
    // запеките сглаженные нормали в UV3 (Toon Pro > SmoothNormalsBaker,
    // канал "UV3") и включите "Сглаженные нормали".
    Properties
    {
        [Header(Base)]
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _OcclusionMap ("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2

        [Header(Emission)]
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)

        [Header(Toon  Shadow)]
        [Toggle(_UNSHADED)] _Unshaded ("Без шейдинга (плоский цвет)", Float) = 0
        _UnshadedBrightness ("Unshaded Brightness", Range(0.0, 2.0)) = 1.0
        _StepThreshold ("Step Threshold", Range(0.0, 1.0)) = 0.45
        _StepSmooth ("Step Smoothness", Range(0.001, 0.1)) = 0.008
        _ShadowColor ("Shadow Tint (доля света в тени)", Color) = (0.72, 0.6, 0.7, 1)
        _ShadowSaturation ("Shadow Saturation (насыщенность тени)", Range(0.0, 1.0)) = 0.7
        _AmbientInfluence ("Ambient Influence", Range(0.0, 1.0)) = 0.35
        _FlatAmbient ("Flat Ambient (без градиента)", Range(0.0, 1.0)) = 1.0
        _LightColorInfluence ("Light Color Influence (тон от ламп)", Range(0.0, 1.0)) = 0.35
        _MinLight ("Min Light (яркость в темноте)", Range(0.0, 1.5)) = 0.85
        _AddLightStep ("Point Light Step", Range(0.0, 1.0)) = 0.3
        _AddLightSoftness ("Point Light Softness (0 toon, 1 мягко)", Range(0.0, 1.0)) = 0.8

        [Header(Rim)]
        _RimColor ("Rim Color", Color) = (0, 0, 0, 1)
        _RimThreshold ("Rim Threshold", Range(0.0, 1.0)) = 0.55
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0

        [Header(Metal)]
        [Toggle(_METAL)] _Metal ("Металл (matcap + блик)", Float) = 0
        [Toggle(_SPECULAR)] _Specular ("Блик (без matcap)", Float) = 0
        _ArcaSpecColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecSize ("Specular Size", Range(0.0, 1.0)) = 0.3
        _MatCap ("MatCap", 2D) = "gray" {}
        _MatCapStrength ("MatCap Strength", Range(0.0, 1.0)) = 0.5

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.04, 0.045, 0.08, 1)
        _OutlineWidthPx ("Outline Width (px)", Range(0.0, 8.0)) = 2.5
        [Toggle(_OUTLINE_SMOOTHNORMALS)] _OutlineSmoothNormals ("Сглаженные нормали (UV3)", Float) = 0

        [Header(Transparency)]
        _FadeAmount ("Fade Amount (скрипт)", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _ArcaOccluder ("Occluder", Float) = 0.0

        [Header(Shadows)]
        [Toggle] _ArcaNoLanternShadow ("Без тени от фонаря Теилс", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 300

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float  _BumpScale;
            float  _OcclusionStrength;
            float4 _EmissionColor;
            float  _UnshadedBrightness;
            float  _StepThreshold;
            float  _StepSmooth;
            float4 _ShadowColor;
            float  _ShadowSaturation;
            float  _AmbientInfluence;
            float  _FlatAmbient;
            float  _LightColorInfluence;
            float  _MinLight;
            float  _AddLightStep;
            float  _AddLightSoftness;
            float4 _RimColor;
            float  _RimThreshold;
            float  _RimPower;
            float4 _ArcaSpecColor;
            float  _SpecSize;
            float  _MatCapStrength;
            float4 _OutlineColor;
            float  _OutlineWidthPx;
            float  _FadeAmount;
            float  _ArcaOccluder;
            float  _ArcaNoLanternShadow;
        CBUFFER_END

        // ShadowCaster умеет пропускать тень от фонаря Теилс (ArcaPasses.hlsl).
        #define ARCA_LANTERN_SHADOW_SKIP 1

        #include "Common/ArcaGlobals.hlsl"
        #include "Common/ARCADither.hlsl"
        #include "Common/ArcaVision.hlsl"
        #include "Common/ArcaOcclusion.hlsl"
        ENDHLSL

        // PASS 1: Forward
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _METAL
            #pragma shader_feature_local _SPECULAR
            #pragma shader_feature_local_fragment _UNSHADED

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            // SSAO на персонажах не применяется (грязнит рисованную заливку);
            // контактные тени на полу от них остаются - DepthNormals пишется.
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Common/ArcaLighting.hlsl"

            TEXTURE2D(_BaseMap);      SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);      SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap); SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_EmissionMap);  SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_MatCap);       SAMPLER(sampler_MatCap);

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
                ArcaClipOcclusion(input.positionCS, input.positionWS, 0.0, true);

                half4 baseMap  = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                half  occ      = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, input.uv).g;
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;

                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 normalWS = normalize(mul(normalTS, tangentToWorld));

                half3 albedo = baseMap.rgb * _BaseColor.rgb;

                ArcaToonParams tp   = ArcaDefaultToonParams();
                tp.stepThreshold    = _StepThreshold;
                tp.stepSmooth       = _StepSmooth;
                tp.shadowColor      = _ShadowColor.rgb;
                tp.ambientInfluence = _AmbientInfluence;
                tp.addLightStep     = _AddLightStep;
                tp.addLightSoftness = _AddLightSoftness;
                tp.rimColor         = _RimColor.rgb;
                tp.rimThreshold     = _RimThreshold;
                tp.rimPower         = _RimPower;
                tp.shadowSaturation    = _ShadowSaturation;
                tp.flatAmbient         = _FlatAmbient;
                tp.lightColorInfluence = _LightColorInfluence;
                tp.minLight            = _MinLight;

                #if defined(_METAL) || defined(_SPECULAR)
                    tp.specColor = _ArcaSpecColor.rgb;
                    tp.specSize  = _SpecSize;
                #endif

                #if defined(_METAL)
                    // MatCap по нормали в пространстве камеры - "отражение" металла
                    float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
                    half3  matcap   = SAMPLE_TEXTURE2D(_MatCap, sampler_MatCap, normalVS.xy * 0.5 + 0.5).rgb;
                    albedo *= lerp(half3(1, 1, 1), matcap * 2.0, _MatCapStrength);
                #endif

                ArcaSurface s;
                s.albedo     = albedo;
                s.occlusion  = lerp(1.0, occ, _OcclusionStrength);
                s.positionWS = input.positionWS;
                s.normalWS   = normalWS;
                s.positionCS = input.positionCS;

                #if defined(_UNSHADED)
                    // Шейдинг выключен: ни света, ни теней, ни ambient - чистая заливка.
                    half3 color = albedo * _UnshadedBrightness;
                #else
                    half3 color = ArcaToonLighting(s, tp);
                #endif

                float vis = ArcaVisionFactor(input.positionWS, input.positionCS);
                color  = ArcaApplyCatVision(color, vis);
                color += ArcaVisionEmission(emission, vis);

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

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _OUTLINE_SMOOTHNORMALS
            #pragma multi_compile_fog
            #include "Common/ArcaOutline.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float3 smoothNormalOS : TEXCOORD3; // SmoothNormalsBaker, канал UV3
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half   fogFactor  : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                #if defined(_OUTLINE_SMOOTHNORMALS)
                    float3 n = input.smoothNormalOS;
                #else
                    float3 n = input.normalOS;
                #endif
                output.positionCS = ArcaOutlinePositionCS(input.positionOS.xyz, n, _OutlineWidthPx);
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
            Cull [_Cull]

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
            Cull [_Cull]

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
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ArcaDepthVert
            #pragma fragment ArcaDepthNormalsFrag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include "Common/ArcaPasses.hlsl"
            ENDHLSL
        }
    }
}

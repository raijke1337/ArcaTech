Shader "Stylized/Toon Wall"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (0.7, 0.7, 0.7, 1)
        [MainTexture] _MainTex ("Albedo", 2D) = "white" {}

        [Header(Step Shading)]
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.25, 1)
        _StepOffset ("Step Offset", Range(-0.5, 0.5)) = 0
        _StepSoftness ("Step Softness", Range(0.001, 0.2)) = 0.01
        [Toggle] _UseShadows ("Receive Shadows", Float) = 1
        _MainLightIntensity ("Main Light Intensity", Range(0, 4)) = 1

        [Header(Outline)]
        [Toggle(_USEOUTLINE_ON)] _UseOutline ("Use Outline", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _Thickness ("Thickness", Range(0, 0.1)) = 0.02
        _AdaptiveThickness ("Adaptive Thickness", Range(0, 1)) = 0.3
        _OutlineTextureStrength ("Outline Texture Strength", Range(0, 1)) = 0

        [Header(Transparency)]
        // 0 = Opaque, 1 = Transparent (для fade стен)
        [Enum(Opaque,0,Transparent,1)] _Surface ("Surface Type", Float) = 1
        [HideInInspector] _SrcBlend ("Src Blend", Float) = 5 // SrcAlpha
        [HideInInspector] _DstBlend ("Dst Blend", Float) = 10 // OneMinusSrcAlpha
        [HideInInspector] _ZWrite ("ZWrite", Float) = 0
        [HideInInspector] _Cull ("Cull", Float) = 2 // Back

        [HideInInspector] _QueueOffset ("Queue Offset", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        // -------------------------------------------------
        // Pass 0: Outline (inverted hull)
        // -------------------------------------------------
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _USEOUTLINE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _ShadowColor;
                float4 _OutlineColor;
                float _StepOffset;
                float _StepSoftness;
                float _UseShadows;
                float _MainLightIntensity;
                float _Thickness;
                float _AdaptiveThickness;
                float _OutlineTextureStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                #ifdef _USEOUTLINE_ON
                    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                    float dist = distance(_WorldSpaceCameraPos, positionWS);
                    float thickness = _Thickness * lerp(1.0, dist, _AdaptiveThickness);

                    float3 normalOS = normalize(input.normalOS);
                    float3 posOS = input.positionOS.xyz + normalOS * thickness;
                    o.positionCS = TransformObjectToHClip(posOS);
                #else
                    // вырожденный треугольник — outline выключен
                    o.positionCS = float4(0, 0, 0, 0);
                #endif

                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                #ifndef _USEOUTLINE_ON
                    discard;
                #endif

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 col = lerp(_OutlineColor.rgb, tex.rgb * _OutlineColor.rgb, _OutlineTextureStrength);

                // alpha берём из _Color — fade через MaterialPropertyBlock
                half alpha = _Color.a * tex.a * _OutlineColor.a;
                return half4(col, alpha);
            }
            ENDHLSL
        }

        // -------------------------------------------------
        // Pass 1: Forward (step shading)
        // -------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZWrite [_ZWrite]
            ZTest LEqual
            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _ShadowColor;
                float4 _OutlineColor;
                float _StepOffset;
                float _StepSoftness;
                float _UseShadows;
                float _MainLightIntensity;
                float _Thickness;
                float _AdaptiveThickness;
                float _OutlineTextureStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord : TEXCOORD3;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(input.normalOS);

                o.positionCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.normalWS   = nrmInputs.normalWS;
                o.uv         = TRANSFORM_TEX(input.uv, _MainTex);

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    o.shadowCoord = GetShadowCoord(posInputs);
                #endif

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                half3 normalWS = normalize(i.normalWS);

                // Shadow coord
                float4 shadowCoord = float4(0, 0, 0, 0);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    shadowCoord = i.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                #endif

                Light mainLight = GetMainLight(shadowCoord);

                // N·L
                half NdotL = saturate(dot(normalWS, mainLight.direction));

                // Step shading
                half threshold = 0.5 + _StepOffset;
                half stepShade = smoothstep(threshold - _StepSoftness, threshold + _StepSoftness, NdotL);

                // Shadows
                half shadow = (_UseShadows > 0.5) ? mainLight.shadowAttenuation * mainLight.distanceAttenuation : 1.0;
                stepShade *= shadow;

                // Light color
                half3 lightCol = mainLight.color * _MainLightIntensity;

                // Final diffuse: lerp shadow color → lit color
                half3 lit = albedo.rgb * lightCol;
                half3 col = lerp(_ShadowColor.rgb * albedo.rgb, lit, stepShade);

                // Ambient (лёгкий, чтобы стены в тени не были чёрными)
                half3 ambient = SampleSH(normalWS) * albedo.rgb * 0.35;
                col += ambient;

                // Additional lights (простой add, без step — можно убрать если не нужны)
                #ifdef _ADDITIONAL_LIGHTS
                uint count = GetAdditionalLightsCount();
                for (uint li = 0; li < count; li++)
                {
                    Light l = GetAdditionalLight(li, i.positionWS);
                    half addNdotL = saturate(dot(normalWS, l.direction));
                    half addStep = smoothstep(threshold - _StepSoftness, threshold + _StepSoftness, addNdotL);
                    col += albedo.rgb * l.color * (l.distanceAttenuation * l.shadowAttenuation) * addStep;
                }
                #endif

                return half4(col, albedo.a);
            }
            ENDHLSL
        }

        // -------------------------------------------------
        // ShadowCaster
        // -------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _ShadowColor;
                float4 _OutlineColor;
                float _StepOffset;
                float _StepSoftness;
                float _UseShadows;
                float _MainLightIntensity;
                float _Thickness;
                float _AdaptiveThickness;
                float _OutlineTextureStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }

            Varyings vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                o.positionCS = GetShadowPositionHClip(input);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // При полной прозрачности можно не отбрасывать тень — опционально:
                // if (_Color.a < 0.01) discard;
                return 0;
            }
            ENDHLSL
        }

        // -------------------------------------------------
        // DepthOnly
        // -------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _ShadowColor;
                float4 _OutlineColor;
                float _StepOffset;
                float _StepSoftness;
                float _UseShadows;
                float _MainLightIntensity;
                float _Thickness;
                float _AdaptiveThickness;
                float _OutlineTextureStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
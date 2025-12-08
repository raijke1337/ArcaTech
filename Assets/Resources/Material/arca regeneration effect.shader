Shader "Arcatech/HealingEffect"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        
        [Header(Healing Effect)]
        _HealColor ("Heal Color", Color) = (0.2, 1, 0.5, 1)
        _HealColor2 ("Heal Color 2", Color) = (0.5, 1, 0.8, 1)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveFrequency ("Wave Frequency", Float) = 3.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.3
        _Brightness ("Brightness", Range(0, 3)) = 1.5
        
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.3, 1, 0.6, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
        _OutlineGlow ("Outline Glow", Range(0, 5)) = 2.0
        
        [Header(Animation)]
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            Name "HealingEffect"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _HealColor;
                float4 _HealColor2;
                float4 _OutlineColor;
                float _WaveSpeed;
                float _WaveFrequency;
                float _WaveAmplitude;
                float _Brightness;
                float _OutlineWidth;
                float _OutlineGlow;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Calculate vertical wave pattern
                float time = _Time.y * _WaveSpeed;
                float wave = sin((input.positionWS.y + time) * _WaveFrequency) * 0.5 + 0.5;
                wave = pow(wave, 2.0); // Make waves more defined
                
                // Add horizontal variation
                float horizontalWave = sin((input.positionWS.x + input.positionWS.z) * 2.0 + time * 0.5) * 0.5 + 0.5;
                wave = lerp(wave, wave * horizontalWave, _WaveAmplitude);
                
                // Pulse effect
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse = lerp(1.0, pulse, _PulseAmount);
                
                // Mix between two heal colors based on wave
                half4 healColor = lerp(_HealColor, _HealColor2, wave);
                healColor.rgb *= _Brightness * pulse;
                
                // Add extra glow on wave peaks
                float wavePeak = pow(wave, 4.0) * 2.0;
                healColor.rgb += wavePeak * _HealColor2.rgb;
                
                // Fresnel effect for edge glow
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                float fresnel = 1.0 - saturate(dot(input.normalWS, viewDirWS));
                fresnel = pow(fresnel, 3.0);
                
                healColor.rgb += fresnel * _OutlineColor.rgb * _OutlineGlow * 0.5;
                
                // Apply fog
                healColor.rgb = MixFog(healColor.rgb, input.fogCoord);
                
                return healColor;
            }
            ENDHLSL
        }
        
        // Outline Pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float fogCoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineGlow;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // Pulse the outline
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse = lerp(1.0, pulse, _PulseAmount * 0.5);
                
                float3 normalOS = normalize(input.normalOS);
                float3 positionOS = input.positionOS.xyz + normalOS * _OutlineWidth * pulse;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.positionCS = positionInputs.positionCS;
                output.fogCoord = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Pulsing glow
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                
                half4 color = _OutlineColor;
                color.rgb *= _OutlineGlow * (1.0 + pulse * 0.5);
                
                // Apply fog
                color.rgb = MixFog(color.rgb, input.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
        
        // Shadow Caster Pass (for proper shadows)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
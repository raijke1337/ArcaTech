Shader "Custom/URPPlasmaSwordTrail"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0.2, 0.8, 1.0, 1.0)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.0
        _FadeStart ("Fade Start (UV Along Length)", Range(0, 1)) = 0.1
        _FadeEnd ("Fade End (UV Along Length)", Range(0, 1)) = 0.9
        _WidthFade ("Width Fade (Taper Edges)", Range(0, 1)) = 0.2
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 5.0
        _NoiseSpeed ("Noise Speed", Range(0, 10)) = 2.0
        _NoiseIntensity ("Noise Intensity", Range(0, 2)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        
        ZWrite off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        
        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog // optional fog support
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // Simple 2D value noise function for procedural plasma flicker
            float2 hash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep interpolation
                
                float a = dot(hash(i + float2(0.0, 0.0)), f - float2(0.0, 0.0));
                float b = dot(hash(i + float2(1.0, 0.0)), f - float2(1.0, 0.0));
                float c = dot(hash(i + float2(0.0, 1.0)), f - float2(0.0, 1.0));
                float d = dot(hash(i + float2(1.0, 1.0)), f - float2(1.0, 1.0));
                
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y) * 2.0 - 1.0; // -1 to 1 range
            }
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1; // for noise if world-space needed, but UV-based here
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
                float fogCoord : TEXCOORD2; // optional
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                half _GlowIntensity;
                half _FadeStart;
                half _FadeEnd;
                half _WidthFade;
                half _NoiseScale;
                half _NoiseSpeed;
                half _NoiseIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.positionWS = vertexInput.positionWS;
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z); // optional
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Assume UV.x is along trail length (0-1), UV.y is width (0-1)
                half lengthUV = input.uv.x;
                half widthUV = input.uv.y * 2.0 - 1.0; // Center and normalize to [-1,1]
                
                // Linear fade along length: full intensity between _FadeStart and _FadeEnd
                half lengthFade = saturate((lengthUV - _FadeStart) / (_FadeEnd - _FadeStart));
                lengthFade *= saturate((1.0 - lengthUV) / (1.0 - _FadeEnd)); // Fade out the end if needed
                
                // Taper width: smoothstep for soft edges
                half widthMask = 1.0 - abs(widthUV);
                widthMask = smoothstep(_WidthFade, 1.0, widthMask);
                
                // Procedural noise for plasma effect (UV-based with time animation)
                float2 noiseUV = input.uv * _NoiseScale;
                noiseUV.x += lengthUV * 0.5; // Slight scroll along length for trail flow
                noiseUV.y += _Time.y * _NoiseSpeed; // Animate vertically for flicker
                
                half n = noise(noiseUV) * _NoiseIntensity; // -_NoiseIntensity to +_NoiseIntensity
                
                // Apply noise to modulate glow (flicker intensity) and slight alpha variation
                half modulatedFade = lengthFade * (1.0 + n * 0.3); // Subtle alpha flicker
                half alpha = modulatedFade * widthMask;
                half glowMod = 1.0 + _GlowIntensity * lengthFade * (1.0 + n); // Noise-driven glow variation
                
                half4 color = _MainColor * glowMod;
                color.a = saturate(alpha); // Clamp alpha
                
                // Apply fog if enabled
                #ifdef _FOG_FRAGMENT
                    color.rgb = MixFog(color.rgb, input.fogCoord);
                #endif
                
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Unlit"
}
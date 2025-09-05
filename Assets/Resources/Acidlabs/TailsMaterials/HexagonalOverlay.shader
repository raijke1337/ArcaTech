Shader "Custom/URP/PulseRevealHexagonalOverlay"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 0.5)
        _HexColor ("Hexagon Color", Color) = (0, 0, 0, 1)
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1)
        _HexScale ("Hexagon Scale", Float) = 10.0
        _HexThickness ("Hexagon Line Thickness", Range(0.01, 5)) = 0.05
        _Alpha ("Overall Alpha", Range(0, 1)) = 0.7
        
        [Header(Animation)]
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseIntensity ("Pulse Intensity", Range(0, 5)) = 1.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveFrequency ("Wave Frequency", Float) = 5.0
        _WaveAmplitude ("Wave Amplitude", Range(0, 3)) = 1.0
        _NoiseScale ("Noise Scale", Float) = 20.0
        _NoiseSpeed ("Noise Speed", Float) = 0.5
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 2.0
        
        [Header(Hex Reveal)]
        _RevealThreshold ("Reveal Threshold", Range(0, 1)) = 0.3
        _RevealSoftness ("Reveal Softness", Range(0.01, 0.5)) = 0.1
        _HexFadeSpeed ("Hex Fade Speed", Range(0.1, 5)) = 1.0
        
        [Header(Outline)]
        [Toggle(_OUTLINE_ON)] _EnableOutline ("Enable Outline", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.005
        _OutlineAlpha ("Outline Alpha", Range(0, 1)) = 1.0
        [Toggle] _OutlineAnimated ("Animate Outline", Float) = 0
        
        [Header(HDR)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        // Outline Pass (renders first)
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #pragma shader_feature_local _OUTLINE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesOutline
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsOutline
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _HexColor;
                half4 _GlowColor;
                half4 _EmissionColor;
                half4 _OutlineColor;
                float _HexScale;
                float _HexThickness;
                float _Alpha;
                float _PulseSpeed;
                float _PulseIntensity;
                float _WaveSpeed;
                float _WaveFrequency;
                float _WaveAmplitude;
                float _NoiseScale;
                float _NoiseSpeed;
                float _GlowIntensity;
                float _RevealThreshold;
                float _RevealSoftness;
                float _HexFadeSpeed;
                float _OutlineWidth;
                float _OutlineAlpha;
                float _OutlineAnimated;
            CBUFFER_END

            // Animation functions for outline
            float calculateOutlineGlow(float2 uv, float3 worldPos, float time)
            {
                if (_OutlineAnimated < 0.5) return 1.0;
                
                float pulse = sin(time * _PulseSpeed * 1.5) * 0.5 + 0.5;
                float wave = sin(worldPos.y * 3.0 + time * 2.0) * 0.3 + 0.7;
                return pulse * wave;
            }

            VaryingsOutline vertOutline(AttributesOutline input)
            {
                VaryingsOutline output = (VaryingsOutline)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Calculate outline animation
                float time = _Time.y;
                float outlineGlow = calculateOutlineGlow(input.uv, input.positionOS.xyz, time);
                
                // Expand vertex along normal for outline effect
                float3 normalOS = normalize(input.normalOS);
                float3 expandedPos = input.positionOS.xyz + normalOS * _OutlineWidth * outlineGlow;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(expandedPos);
                output.positionHCS = vertexInput.positionCS;
                output.worldPos = vertexInput.positionWS;
                output.uv = input.uv;

                return output;
            }

            half4 fragOutline(VaryingsOutline input) : SV_Target
            {
                #ifndef _OUTLINE_ON
                    discard;
                #endif
                
                float time = _Time.y;
                float outlineGlow = calculateOutlineGlow(input.uv, input.worldPos, time);
                
                half4 outlineColor = _OutlineColor;
                outlineColor.a *= _OutlineAlpha * outlineGlow;
                
                return outlineColor;
            }
            ENDHLSL
        }

        // Main Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _OUTLINE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _HexColor;
                half4 _GlowColor;
                half4 _EmissionColor;
                half4 _OutlineColor;
                float _HexScale;
                float _HexThickness;
                float _Alpha;
                float _PulseSpeed;
                float _PulseIntensity;
                float _WaveSpeed;
                float _WaveFrequency;
                float _WaveAmplitude;
                float _NoiseScale;
                float _NoiseSpeed;
                float _GlowIntensity;
                float _RevealThreshold;
                float _RevealSoftness;
                float _HexFadeSpeed;
                float _OutlineWidth;
                float _OutlineAlpha;
                float _OutlineAnimated;
            CBUFFER_END

            // Simple noise function
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(hash(i + float2(0.0, 0.0)), 
                                hash(i + float2(1.0, 0.0)), u.x),
                           lerp(hash(i + float2(0.0, 1.0)), 
                                hash(i + float2(1.0, 1.0)), u.x), u.y);
            }

            // Hexagonal pattern functions
            float2 hex(float2 p)
            {
                p.x *= 0.57735 * 2.0; // sqrt(3)/3 * 2
                p.y += fmod(floor(p.x), 2.0) * 0.5;
                p = abs(frac(p) - 0.5);
                return abs(max(p.x * 1.5 + p.y, p.y * 2.0) - 1.0);
            }

            float hexagon(float2 uv, float scale, float thickness)
            {
                float2 grid = uv * scale;
                float2 hexUV = hex(grid);
                
                // Create hexagon outline
                float dist = length(hexUV);
                float hexPattern = smoothstep(thickness, thickness * 0.5, dist);
                
                return hexPattern;
            }

            // Animation functions
            float calculateGlow(float2 uv, float3 worldPos, float time)
            {
                // Base pulse animation
                float pulse = sin(time * _PulseSpeed) * 0.5 + 0.5;
                pulse = pow(pulse, 2.0) * _PulseIntensity;
                
                // Wave animation across surface
                float wave = sin(worldPos.x * _WaveFrequency + time * _WaveSpeed) * 
                            sin(worldPos.z * _WaveFrequency * 0.7 + time * _WaveSpeed * 0.8);
                wave = wave * 0.5 + 0.5;
                wave = pow(wave, 1.5) * _WaveAmplitude;
                
                // Noise-based random glow spots
                float2 noiseUV = uv * _NoiseScale + time * _NoiseSpeed;
                float noiseValue = noise(noiseUV);
                float noiseGlow = smoothstep(0.6, 0.9, noiseValue) * 2.0;
                
                // Secondary noise layer for more variation
                float2 noiseUV2 = uv * _NoiseScale * 0.5 + time * _NoiseSpeed * 1.3;
                float noiseValue2 = noise(noiseUV2);
                float noiseGlow2 = smoothstep(0.7, 1.0, noiseValue2) * 1.5;
                
                // Combine all glow effects
                float totalGlow = pulse + wave + noiseGlow + noiseGlow2;
                return saturate(totalGlow);
            }

            // Calculate hex reveal based on animation intensity
            float calculateHexReveal(float glowAmount, float time, float2 uv)
            {
                // Create a reveal mask based on glow intensity
                float revealMask = smoothstep(_RevealThreshold - _RevealSoftness, 
                                            _RevealThreshold + _RevealSoftness, 
                                            glowAmount);
                
                // Add some temporal fade for smoother transitions
                float fadeMask = saturate(glowAmount * _HexFadeSpeed);
                
                // Add slight per-hexagon variation
                float2 hexCenter = floor(uv * _HexScale) / _HexScale;
                float hexNoise = hash(hexCenter) * 0.3;
                
                float finalReveal = revealMask * fadeMask * (1.0 + hexNoise);
                return saturate(finalReveal);
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                output.worldPos = vertexInput.positionWS;
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                
                // Calculate animated glow first
                float glowAmount = calculateGlow(input.uv, input.worldPos, time);
                
                // Calculate hex reveal based on glow intensity
                float hexReveal = calculateHexReveal(glowAmount, time, input.uv);
                
                // Generate hexagonal pattern only if revealed
                float hexPattern = 0.0;
                if (hexReveal > 0.01) // Only calculate hex pattern if it might be visible
                {
                    hexPattern = hexagon(input.uv, _HexScale, _HexThickness) * hexReveal;
                }
                
                // Start with base color (always visible)
                half4 finalColor = _BaseColor;
                
                // Only add hexagon effects where they're revealed
                if (hexPattern > 0.01)
                {
                    // Create glow effect on revealed hexagon lines
                    float hexGlow = hexPattern * glowAmount * _GlowIntensity;
                    
                    // Mix colors based on glow intensity
                    half4 baseHexColor = lerp(_HexColor, _GlowColor, saturate(hexGlow));
                    
                    // Blend hexagon color with base color
                    finalColor = lerp(finalColor, baseHexColor, hexPattern);
                    
                    // Add emission glow only to revealed hexagons
                    half3 emission = _EmissionColor.rgb * hexGlow * hexPattern;
                    finalColor.rgb += emission;
                    
                    // Boost alpha where hexagons are glowing
                    finalColor.a += hexGlow * hexPattern * 0.5;
                }
                
                // Apply overall alpha
                finalColor.a *= _Alpha;
                finalColor.a = saturate(finalColor.a);
                
                return finalColor;
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
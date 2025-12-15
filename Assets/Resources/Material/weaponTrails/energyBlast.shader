Shader "Custom/CrescentEnergyWave"
{
    Properties
    {
        _MainColor ("Crescent Fill Color", Color) = (0.2, 0.8, 1.0, 1.0)  // Matching plasma: brighter blue-cyan
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.0  // From plasma: modulates the noise-driven glow
        _OutlineColor ("Outer Outline Color", Color) = (0.0, 0.8, 1.0, 1.0)  // Brighter blue/cyan for cartoony sharp outline
        _LargeRadius ("Large Circle Radius", Float) = 0.5
        _SmallRadius ("Small Circle Radius", Float) = 0.4
        _OffsetX ("Small Circle Base Offset X", Float) = 0.25  // Base offset to control crescent thinness
        _OuterEdgeSharpness ("Outer Edge Sharpness", Range(0.001, 0.05)) = 0.005  // Low for cartoony hard edges
        _InnerEdgeSoftness ("Inner Edge Softness", Range(0.001, 0.05)) = 0.01  // For smooth distorted inner edge
        // Noise (matching plasma options + distortion control)
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 5.0  // From plasma
        _NoiseSpeed ("Noise Speed", Range(0, 10)) = 2.0  // From plasma
        _NoiseIntensity ("Noise Intensity", Range(0, 2)) = 0.5  // From plasma: for inner color glow modulation
        _DistortionStrength ("Inner Distortion Strength", Range(0, 0.1)) = 0.04  // Controls edge waviness (tied to same noise)
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        _PulseAmplitude ("Pulse Amplitude", Range(0.0, 0.2)) = 0.05  // For subtle size breathing
        _OffsetOscillation ("Offset Oscillation Amplitude", Range(0.0, 0.1)) = 0.03  // For shifting the crescent
        _OutlineWidth ("Outer Outline Width", Range(0.001, 0.05)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                half _GlowIntensity;
                float4 _OutlineColor;
                float _LargeRadius;
                float _SmallRadius;
                float _OffsetX;
                float _OuterEdgeSharpness;
                float _InnerEdgeSoftness;
                float _NoiseScale;
                float _NoiseSpeed;
                half _NoiseIntensity;
                float _DistortionStrength;
                float _PulseSpeed;
                float _PulseAmplitude;
                float _OffsetOscillation;
                float _OutlineWidth;
            CBUFFER_END

            // Noise function copied exactly from plasma shader (Perlin-like value noise, -1 to 1 range)
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

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;

                // Animated parameters
                float animatedOffsetX = _OffsetX + sin(time * 1.5) * _OffsetOscillation;  // Gentle left-right shift
                float pulse = 1.0 + sin(time * _PulseSpeed) * _PulseAmplitude;  // Size breathing
                float animatedLargeRadius = _LargeRadius * pulse;
                float animatedSmallRadius = _SmallRadius * pulse;

                // Centers of the two circles (assuming UVs span 0-1)
                float2 centerLarge = float2(0.5, 0.5);
                float2 centerSmall = float2(0.5 + animatedOffsetX, 0.5);

                // Procedural noise (matching plasma: UV-based with time animation for flicker)
                float2 noiseUV = input.uv * _NoiseScale;
                // Adapt plasma's animation: slight "flow" in X (using distance for radial-ish), Y time-based
                float distFromCenter = distance(input.uv, centerLarge);
                noiseUV.x += distFromCenter * 0.5;  // Gentle radial flow for energy wave feel
                noiseUV.y += time * _NoiseSpeed;  // Vertical flicker like plasma trail
                float rawNoise = noise(noiseUV);  // -1 to 1

                // For inner color modulation (matching plasma)
                half nColor = rawNoise * _NoiseIntensity;  // -_NoiseIntensity to +_NoiseIntensity
                half glowMod = 1.0 + _GlowIntensity * (1.0 + nColor);  // Plasma-style glow variation
                half alphaFlicker = 1.0 + nColor * 0.3;  // Subtle alpha variation like plasma's modulatedFade

                // For inner edge distortion (using same noise, scaled separately)
                float nDistort = rawNoise * _DistortionStrength;  // Small displacement for waviness

                // Distances to centers
                float distLarge = distance(input.uv, centerLarge);
                float distSmall = distance(input.uv, centerSmall);

                // Signed distance fields (SDF):
                // Outer (large): Clean, no noise for sharp, undistorted edge and outline
                float sdfLarge = distLarge - animatedLargeRadius;
                // Inner (small): With noise for distorted edge
                float sdfSmall = (distSmall - animatedSmallRadius) + nDistort;

                // Crescent region: inside clean large circle AND outside noisy small circle
                // Outer edge: Sharp (low softness)
                float insideLarge = 1.0 - smoothstep(-_OuterEdgeSharpness, _OuterEdgeSharpness, sdfLarge);
                // Inner edge: Softer, to accommodate distortion
                float outsideSmall = smoothstep(-_InnerEdgeSoftness, _InnerEdgeSoftness, sdfSmall);
                float crescentMask = insideLarge * outsideSmall;

                // Outer outline: Only on the large circle boundary, sharp and clean (no noise)
                // Show outline where near the outer edge and inside the large circle (to follow the arc)
                float outlineMask = 0.0;
                float insideLargeForOutline = 1.0 - step(0.0, sdfLarge);  // Hard inside for precise boundary
                float outlineStroke = smoothstep(_OutlineWidth, 0.0, abs(sdfLarge)) * step(0.0, sdfLarge + _OutlineWidth);  // Stroke just outside, but masked inside
                outlineMask = outlineStroke * insideLargeForOutline * outsideSmall;  // Only where crescent would be (avoids full circle outline)

                // Output: Fill with main color (plasma-style modulation)
                float4 finalColor = _MainColor;
                finalColor.rgb *= glowMod;  // Glow modulation
                finalColor.a = crescentMask * alphaFlicker;  // Mask with subtle flicker

                // Clamp for clean rendering
                finalColor.a = saturate(finalColor.a);
                finalColor.rgb = saturate(finalColor.rgb);

                // Overlay outer outline (overrides fill where present; no modulation needed for outline)
                if (outlineMask > 0.0)
                {
                    finalColor = _OutlineColor;
                    finalColor.a = outlineMask;
                }

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
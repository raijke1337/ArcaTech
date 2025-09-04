Shader "Custom/URP/FuturisticArmorHexOverlay"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.8, 0.9, 1, 0.9)
        _HexColor ("Hexagon Color", Color) = (0, 0.5, 1, 1)
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1)
        _HexScale ("Hexagon Scale", Float) = 10.0
        _HexThickness ("Hexagon Line Thickness", Range(0.01, 0.2)) = 0.05
        _Alpha ("Overall Alpha", Range(0, 1)) = 0.9
        
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
        
        [Header(Armor Details)]
        _PanelColor ("Panel Line Color", Color) = (0.2, 0.3, 0.4, 1)
        _PanelScale ("Panel Scale", Float) = 2.0
        _PanelWidth ("Panel Line Width", Range(0.005, 0.05)) = 0.02
        _CircuitColor ("Circuit Color", Color) = (0, 1, 0.5, 1)
        _CircuitScale ("Circuit Scale", Float) = 15.0
        _CircuitWidth ("Circuit Width", Range(0.002, 0.02)) = 0.008
        _CircuitGlow ("Circuit Glow", Range(0, 5)) = 1.5
        
        [Header(Fabric Texture)]
        _FabricScale ("Fabric Scale", Float) = 50.0
        _FabricStrength ("Fabric Strength", Range(0, 0.3)) = 0.1
        _FabricColor ("Fabric Tint", Color) = (1, 1, 1, 1)
        
        [Header(Metallic Accents)]
        _MetallicColor ("Metallic Color", Color) = (0.9, 0.95, 1, 1)
        _MetallicScale ("Metallic Strip Scale", Float) = 8.0
        _MetallicWidth ("Metallic Strip Width", Range(0.05, 0.3)) = 0.15
        _MetallicReflection ("Metallic Reflection", Range(0, 2)) = 1.2
        
        [Header(Rim Lighting)]
        _RimColor ("Rim Color", Color) = (0.5, 0.8, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 8)) = 2.0
        _RimIntensity ("Rim Intensity", Range(0, 3)) = 1.0
        
        [Header(Wear and Weathering)]
        _WearColor ("Wear Color", Color) = (0.6, 0.7, 0.8, 1)
        _WearScale ("Wear Scale", Float) = 25.0
        _WearAmount ("Wear Amount", Range(0, 1)) = 0.3
        
        [Header(Outline)]
        [Toggle(_OUTLINE_ON)] _EnableOutline ("Enable Outline", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0, 0.3, 0.6, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.02)) = 0.008
        _OutlineAlpha ("Outline Alpha", Range(0, 1)) = 0.8
        
        [Header(HDR)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0.5, 1, 1)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        // Outline Pass
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
                half4 _PanelColor;
                half4 _CircuitColor;
                half4 _FabricColor;
                half4 _MetallicColor;
                half4 _RimColor;
                half4 _WearColor;
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
                float _PanelScale;
                float _PanelWidth;
                float _CircuitScale;
                float _CircuitWidth;
                float _CircuitGlow;
                float _FabricScale;
                float _FabricStrength;
                float _MetallicScale;
                float _MetallicWidth;
                float _MetallicReflection;
                float _RimPower;
                float _RimIntensity;
                float _WearScale;
                float _WearAmount;
                float _OutlineWidth;
                float _OutlineAlpha;
            CBUFFER_END

            VaryingsOutline vertOutline(AttributesOutline input)
            {
                VaryingsOutline output = (VaryingsOutline)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 normalOS = normalize(input.normalOS);
                float3 expandedPos = input.positionOS.xyz + normalOS * _OutlineWidth;
                
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
                
                return half4(_OutlineColor.rgb, _OutlineColor.a * _OutlineAlpha);
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _HexColor;
                half4 _GlowColor;
                half4 _EmissionColor;
                half4 _OutlineColor;
                half4 _PanelColor;
                half4 _CircuitColor;
                half4 _FabricColor;
                half4 _MetallicColor;
                half4 _RimColor;
                half4 _WearColor;
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
                float _PanelScale;
                float _PanelWidth;
                float _CircuitScale;
                float _CircuitWidth;
                float _CircuitGlow;
                float _FabricScale;
                float _FabricStrength;
                float _MetallicScale;
                float _MetallicWidth;
                float _MetallicReflection;
                float _RimPower;
                float _RimIntensity;
                float _WearScale;
                float _WearAmount;
                float _OutlineWidth;
                float _OutlineAlpha;
            CBUFFER_END

            // Utility functions
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

            // Hexagonal pattern
            float2 hex(float2 p)
            {
                p.x *= 0.57735 * 2.0;
                p.y += fmod(floor(p.x), 2.0) * 0.5;
                p = abs(frac(p) - 0.5);
                return abs(max(p.x * 1.5 + p.y, p.y * 2.0) - 1.0);
            }

            float hexagon(float2 uv, float scale, float thickness)
            {
                float2 grid = uv * scale;
                float2 hexUV = hex(grid);
                float dist = length(hexUV);
                return smoothstep(thickness, thickness * 0.5, dist);
            }

            // Panel lines (armor plating)
            float panelLines(float2 uv)
            {
                float2 grid = uv * _PanelScale;
                float2 gridLines = abs(frac(grid) - 0.5);
                float panelMask = min(gridLines.x, gridLines.y);
                return smoothstep(_PanelWidth * 0.5, _PanelWidth, panelMask);
            }

            // Circuit traces
            float circuitPattern(float2 uv, float time)
            {
                float2 grid = uv * _CircuitScale;
                float2 id = floor(grid);
                float2 localUV = frac(grid) - 0.5;
                
                // Create circuit paths
                float h = hash(id);
                float direction = step(0.5, h);
                
                float circuit = 0.0;
                if (direction > 0.5)
                {
                    // Horizontal trace
                    circuit = smoothstep(_CircuitWidth, _CircuitWidth * 0.5, abs(localUV.y));
                    // Add animated energy flow
                    float flow = sin(time * 3.0 + id.x * 2.0) * 0.5 + 0.5;
                    circuit *= 0.3 + flow * 0.7;
                }
                else
                {
                    // Vertical trace
                    circuit = smoothstep(_CircuitWidth, _CircuitWidth * 0.5, abs(localUV.x));
                    float flow = sin(time * 3.0 + id.y * 2.0) * 0.5 + 0.5;
                    circuit *= 0.3 + flow * 0.7;
                }
                
                return circuit * _CircuitGlow;
            }

            // Fabric texture
            float fabricTexture(float2 uv)
            {
                float2 fabricUV = uv * _FabricScale;
                float fabric1 = sin(fabricUV.x * 6.28) * sin(fabricUV.y * 6.28);
                float fabric2 = sin(fabricUV.x * 6.28 * 1.3) * sin(fabricUV.y * 6.28 * 0.7);
                return (fabric1 + fabric2) * 0.5 * _FabricStrength;
            }

            // Metallic strips
            float metallicStrips(float2 uv)
            {
                float2 grid = uv * _MetallicScale;
                float strips = sin(grid.y * 6.28);
                return smoothstep(1.0 - _MetallicWidth, 1.0, abs(strips));
            }

            // Wear and weathering
            float wearPattern(float2 uv)
            {
                float2 wearUV = uv * _WearScale;
                float wear1 = noise(wearUV);
                float wear2 = noise(wearUV * 2.1);
                float wear3 = noise(wearUV * 4.3);
                
                float combinedWear = wear1 * 0.5 + wear2 * 0.3 + wear3 * 0.2;
                return smoothstep(0.6, 1.0, combinedWear) * _WearAmount;
            }

            // Animation calculation
            float calculateGlow(float2 uv, float3 worldPos, float time)
            {
                float pulse = sin(time * _PulseSpeed) * 0.5 + 0.5;
                pulse = pow(pulse, 2.0) * _PulseIntensity;
                
                float wave = sin(worldPos.x * _WaveFrequency + time * _WaveSpeed) * 
                            sin(worldPos.z * _WaveFrequency * 0.7 + time * _WaveSpeed * 0.8);
                wave = wave * 0.5 + 0.5;
                wave = pow(wave, 1.5) * _WaveAmplitude;
                
                float2 noiseUV = uv * _NoiseScale + time * _NoiseSpeed;
                float noiseValue = noise(noiseUV);
                float noiseGlow = smoothstep(0.6, 0.9, noiseValue) * 2.0;
                
                float totalGlow = pulse + wave + noiseGlow;
                return saturate(totalGlow);
            }

            float calculateHexReveal(float glowAmount, float time, float2 uv)
            {
                float revealMask = smoothstep(_RevealThreshold - _RevealSoftness, 
                                            _RevealThreshold + _RevealSoftness, 
                                            glowAmount);
                float fadeMask = saturate(glowAmount * _HexFadeSpeed);
                float2 hexCenter = floor(uv * _HexScale) / _HexScale;
                float hexNoise = hash(hexCenter) * 0.3;
                
                return saturate(revealMask * fadeMask * (1.0 + hexNoise));
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.worldPos = vertexInput.positionWS;
                output.worldNormal = normalInput.normalWS;
                output.viewDir = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                float3 worldNormal = normalize(input.worldNormal);
                float3 viewDir = normalize(input.viewDir);
                
                // Calculate base patterns
                float glowAmount = calculateGlow(input.uv, input.worldPos, time);
                float hexReveal = calculateHexReveal(glowAmount, time, input.uv);
                
                // Start with base armor color
                half4 finalColor = _BaseColor;
                
                // Add fabric texture
                float fabric = fabricTexture(input.uv);
                finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * _FabricColor.rgb, fabric);
                
                // Add panel lines
                float panels = panelLines(input.uv);
                finalColor.rgb = lerp(_PanelColor.rgb, finalColor.rgb, panels);
                
                // Add metallic strips
                float metallic = metallicStrips(input.uv);
                if (metallic > 0.5)
                {
                    // Fresnel effect for metallic parts
                    float fresnel = pow(1.0 - saturate(dot(worldNormal, viewDir)), _MetallicReflection);
                    half3 metallicColor = _MetallicColor.rgb * (1.0 + fresnel);
                    finalColor.rgb = lerp(finalColor.rgb, metallicColor, metallic);
                }
                
                // Add wear and weathering
                float wear = wearPattern(input.uv);
                finalColor.rgb = lerp(finalColor.rgb, _WearColor.rgb, wear);
                
                // Add circuit traces
                float circuits = circuitPattern(input.uv, time);
                finalColor.rgb += _CircuitColor.rgb * circuits;
                
                // Add hexagon overlay where revealed
                float hexPattern = 0.0;
                if (hexReveal > 0.01)
                {
                    hexPattern = hexagon(input.uv, _HexScale, _HexThickness) * hexReveal;
                    if (hexPattern > 0.01)
                    {
                        float hexGlow = hexPattern * glowAmount * _GlowIntensity;
                        half4 baseHexColor = lerp(_HexColor, _GlowColor, saturate(hexGlow));
                        finalColor = lerp(finalColor, baseHexColor, hexPattern);
                        
                        // Add emission glow
                        half3 emission = _EmissionColor.rgb * hexGlow * hexPattern;
                        finalColor.rgb += emission;
                        
                        finalColor.a += hexGlow * hexPattern * 0.3;
                    }
                }
                
                // Add rim lighting for cartoony effect
                float rimFactor = 1.0 - saturate(dot(worldNormal, viewDir));
                float rim = pow(rimFactor, _RimPower) * _RimIntensity;
                finalColor.rgb += _RimColor.rgb * rim;
                
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
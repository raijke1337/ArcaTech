Shader "Custom/URP/EmissiveArmorGlow"
{
    Properties
    {
        [Header(Base Emission)]
        [HDR] _BaseEmission ("Base Emission Color", Color) = (0, 2, 4, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 2.0
        _BaseAlpha ("Base Alpha", Range(0, 1)) = 0.8
        
        [Header(Energy Flow)]
        [HDR] _FlowColor ("Flow Color", Color) = (0, 4, 8, 1)
        _FlowSpeed ("Flow Speed", Float) = 2.0
        _FlowScale ("Flow Scale", Float) = 8.0
        _FlowIntensity ("Flow Intensity", Range(0, 5)) = 1.5
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
        
        [Header(Pulse Animation)]
        _PulseSpeed ("Pulse Speed", Float) = 1.5
        _PulseMin ("Pulse Minimum", Range(0, 1)) = 0.3
        _PulseMax ("Pulse Maximum", Range(1, 3)) = 2.0
        _PulseSharpness ("Pulse Sharpness", Range(0.1, 5)) = 1.0
        
        [Header(Temperature Gradient)]
        [HDR] _CoolColor ("Cool Color", Color) = (0, 1, 4, 1)
        [HDR] _HotColor ("Hot Color", Color) = (4, 2, 0, 1)
        _TemperatureSpeed ("Temperature Speed", Float) = 0.8
        _TemperatureScale ("Temperature Scale", Float) = 12.0
        _TemperatureIntensity ("Temperature Intensity", Range(0, 2)) = 0.7
        
        [Header(Fresnel Glow)]
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 2.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 5)) = 1.5
        [HDR] _FresnelColor ("Fresnel Color", Color) = (0, 6, 12, 1)
        
        [Header(Power Surge)]
        [Toggle] _EnableSurge ("Enable Power Surge", Float) = 1
        _SurgeFrequency ("Surge Frequency", Float) = 0.3
        _SurgeIntensity ("Surge Intensity", Range(0, 10)) = 3.0
        _SurgeDuration ("Surge Duration", Range(0.1, 2)) = 0.5
        [HDR] _SurgeColor ("Surge Color", Color) = (8, 8, 0, 1)
        
        [Header(Edge Glow)]
        _EdgeGlowWidth ("Edge Glow Width", Range(0, 1)) = 0.3
        _EdgeGlowIntensity ("Edge Glow Intensity", Range(0, 3)) = 1.2
        [HDR] _EdgeGlowColor ("Edge Glow Color", Color) = (0, 3, 6, 1)
        
        [Header(Noise Details)]
        _NoiseScale1 ("Noise Scale 1", Float) = 15.0
        _NoiseScale2 ("Noise Scale 2", Float) = 25.0
        _NoiseSpeed1 ("Noise Speed 1", Float) = 1.0
        _NoiseSpeed2 ("Noise Speed 2", Float) = 1.3
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.4
        
        [Header(Distortion)]
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Float) = 2.0
        _DistortionScale ("Distortion Scale", Float) = 20.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

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
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseEmission;
                half4 _FlowColor;
                half4 _CoolColor;
                half4 _HotColor;
                half4 _FresnelColor;
                half4 _SurgeColor;
                half4 _EdgeGlowColor;
                float4 _FlowDirection;
                float _EmissionIntensity;
                float _BaseAlpha;
                float _FlowSpeed;
                float _FlowScale;
                float _FlowIntensity;
                float _PulseSpeed;
                float _PulseMin;
                float _PulseMax;
                float _PulseSharpness;
                float _TemperatureSpeed;
                float _TemperatureScale;
                float _TemperatureIntensity;
                float _FresnelPower;
                float _FresnelIntensity;
                float _EnableSurge;
                float _SurgeFrequency;
                float _SurgeIntensity;
                float _SurgeDuration;
                float _EdgeGlowWidth;
                float _EdgeGlowIntensity;
                float _NoiseScale1;
                float _NoiseScale2;
                float _NoiseSpeed1;
                float _NoiseSpeed2;
                float _NoiseIntensity;
                float _DistortionStrength;
                float _DistortionSpeed;
                float _DistortionScale;
            CBUFFER_END

            // Noise functions
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

            // Fractal noise
            float fractalNoise(float2 p, float time, float scale, float speed)
            {
                float2 uv = p * scale + time * speed;
                float n = 0.0;
                n += noise(uv) * 0.5;
                n += noise(uv * 2.1) * 0.25;
                n += noise(uv * 4.3) * 0.125;
                n += noise(uv * 8.7) * 0.0625;
                return n;
            }

            // Energy flow pattern
            float energyFlow(float2 uv, float3 worldPos, float time)
            {
                // Create flowing streams based on direction
                float2 flowUV = uv * _FlowScale;
                float2 direction = normalize(_FlowDirection.xy);
                
                // Create multiple flow layers
                float flow1 = sin((dot(flowUV, direction) - time * _FlowSpeed) * 6.28);
                float flow2 = sin((dot(flowUV * 1.3, direction) - time * _FlowSpeed * 1.1) * 6.28);
                float flow3 = sin((dot(flowUV * 0.7, direction) - time * _FlowSpeed * 0.9) * 6.28);
                
                // Combine flows with different phases
                float combinedFlow = (flow1 + flow2 * 0.7 + flow3 * 0.5) / 2.2;
                
                // Add noise for organic feeling
                float flowNoise = fractalNoise(uv, time, _NoiseScale1, _NoiseSpeed1);
                combinedFlow = lerp(combinedFlow, combinedFlow * flowNoise, _NoiseIntensity);
                
                return saturate(combinedFlow * 0.5 + 0.5) * _FlowIntensity;
            }

            // Pulse animation
            float calculatePulse(float time)
            {
                float pulse = sin(time * _PulseSpeed * 6.28) * 0.5 + 0.5;
                pulse = pow(pulse, _PulseSharpness);
                return lerp(_PulseMin, _PulseMax, pulse);
            }

            // Temperature gradient
            half3 temperatureGradient(float2 uv, float time)
            {
                float temp = fractalNoise(uv, time, _TemperatureScale, _TemperatureSpeed);
                temp = saturate(temp * _TemperatureIntensity);
                return lerp(_CoolColor.rgb, _HotColor.rgb, temp);
            }

            // Power surge effect
            float powerSurge(float time)
            {
                if (_EnableSurge < 0.5) return 1.0;
                
                float surgeTime = fmod(time, 1.0 / _SurgeFrequency);
                float surgeDuration = _SurgeDuration;
                
                if (surgeTime < surgeDuration)
                {
                    float surgePhase = surgeTime / surgeDuration;
                    float surge = sin(surgePhase * 3.14159);
                    return 1.0 + surge * _SurgeIntensity;
                }
                
                return 1.0;
            }

            // Edge glow calculation
            float calculateEdgeGlow(float3 normal, float3 viewDir, float2 uv)
            {
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                float edgeMask = smoothstep(1.0 - _EdgeGlowWidth, 1.0, fresnel);
                
                // Add some noise to the edge
                float edgeNoise = fractalNoise(uv, _Time.y, _NoiseScale2, _NoiseSpeed2);
                edgeMask *= (0.7 + edgeNoise * 0.3);
                
                return edgeMask * _EdgeGlowIntensity;
            }

            // UV distortion for energy effect
            float2 applyDistortion(float2 uv, float time)
            {
                float2 distortUV = uv * _DistortionScale;
                float2 distortion = float2(
                    sin(distortUV.x + time * _DistortionSpeed) * sin(distortUV.y * 1.3 + time * _DistortionSpeed * 0.7),
                    cos(distortUV.y + time * _DistortionSpeed * 1.1) * cos(distortUV.x * 0.9 + time * _DistortionSpeed)
                );
                
                return uv + distortion * _DistortionStrength;
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
                
                // Apply UV distortion
                float2 distortedUV = applyDistortion(input.uv, time);
                
                // Calculate pulse animation
                float pulse = calculatePulse(time);
                
                // Calculate power surge
                float surge = powerSurge(time);
                
                // Base emission
                half3 emission = _BaseEmission.rgb * _EmissionIntensity * pulse;
                
                // Add energy flow
                float flow = energyFlow(distortedUV, input.worldPos, time);
                emission += _FlowColor.rgb * flow * surge;
                
                // Add temperature gradient
                half3 tempGradient = temperatureGradient(distortedUV, time);
                emission += tempGradient * pulse * 0.5;
                
                // Add fresnel glow
                float fresnel = pow(1.0 - saturate(dot(worldNormal, viewDir)), _FresnelPower);
                emission += _FresnelColor.rgb * fresnel * _FresnelIntensity * pulse;
                
                // Add edge glow
                float edgeGlow = calculateEdgeGlow(worldNormal, viewDir, distortedUV);
                emission += _EdgeGlowColor.rgb * edgeGlow;
                
                // Add surge color during power surges
                if (surge > 1.1)
                {
                    float surgeAmount = (surge - 1.0) / _SurgeIntensity;
                    emission += _SurgeColor.rgb * surgeAmount;
                }
                
                // Create final color
                half4 finalColor = half4(emission, _BaseAlpha);
                
                // Boost alpha based on emission intensity for bloom effect
                float emissionLuminance = dot(emission, half3(0.299, 0.587, 0.114));
                finalColor.a += saturate(emissionLuminance * 0.1);
                
                return finalColor;
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
Shader "URP/FX/EMP_Shock_Unlit"
{
    Properties
    {
        [HDR]_BaseColor("Shock Band Color", Color) = (0.1, 0.7, 1.0, 1.0)
        [HDR]_RimColor("Rim Glow Color", Color) = (0.2, 0.9, 1.0, 1.0)
        [HDR]_ArcColor("Electric Arc Color", Color) = (0.6, 1.0, 1.0, 1.0)

        _GlowIntensity("Global Glow Intensity", Range(0, 10)) = 2.0
        _Opacity("Overall Opacity", Range(0, 1)) = 0.85

        _RimPower("Rim Power", Range(0.5, 8.0)) = 2.5

        _BandWidth("Shock Band Width", Range(0.005, 0.5)) = 0.12
        _BandSpeed("Shock Band Speed", Range(-5, 5)) = 0.75

        _ArcCount("Arc Count (around Y)", Range(1, 64)) = 24
        _ArcSharpness("Arc Sharpness", Range(0.5, 10)) = 3.0
        _ArcSpeed("Arc Rotation Speed", Range(-10, 10)) = 1.5

        _ScanDensity("Scanline Density", Range(1, 64)) = 24
        _ScanSpeed("Scanline Speed", Range(-10, 10)) = 2.0

        _NoiseTex("Noise (R)", 2D) = "white" {}
        _NoiseScale("Noise Scale (Angle, Height)", Vector) = (4, 2, 0, 0)
        _NoiseStrength("Noise Strength", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "EMPShock"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 3.0

            // Core + explicit transforms (some URP versions need SpaceTransforms.hlsl separately)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #ifndef PI
            #define PI 3.14159265359
            #endif

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float4 _ArcColor;

                float _GlowIntensity;
                float _Opacity;

                float _RimPower;

                float _BandWidth;
                float _BandSpeed;

                float _ArcCount;
                float _ArcSharpness;
                float _ArcSpeed;

                float _ScanDensity;
                float _ScanSpeed;

                float2 _NoiseScale;
                float _NoiseStrength;
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
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.positionWS = worldPos;
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv         = IN.uv;
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            float Angle01FromXZ(float2 xz)
            {
                // HLSL atan2(y, x)
                float a = atan2(xz.x, xz.y);           // [-PI, PI]
                a = a / (2.0 * PI) + 0.5;              // [0,1)
                return frac(a);
            }

            float Triangle01(float x)
            {
                float f = frac(x);
                return abs(f * 2.0 - 1.0);
            }

            float SmoothBand(float v, float center, float halfWidth)
            {
                float d = abs(v - center);
                return smoothstep(halfWidth, 0.0, d);
            }

            float3 SafeNormalize(float3 v) { return normalize(v + 1e-5); }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = SafeNormalize(IN.normalWS);
                float3 V = SafeNormalize(_WorldSpaceCameraPos.xyz - IN.positionWS);

                // Unity _Time.x is time in seconds; match older comment behavior
                float t = _Time.x;

                float ang01 = Angle01FromXZ(IN.positionOS.xz);
                float vcoord = saturate(IN.uv.y);

                float2 nUV = float2(ang01 * _NoiseScale.x, vcoord * _NoiseScale.y + t * 0.25);
                float noiseR = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, nUV).r;
                float noise  = (noiseR * 2.0 - 1.0) * _NoiseStrength;

                float arcPhase = ang01 * _ArcCount + t * _ArcSpeed;
                float arcTri   = Triangle01(arcPhase + noise * 0.15);
                float arcs     = pow(saturate(1.0 - arcTri), _ArcSharpness);

                float scan = 0.5 + 0.5 * sin((vcoord * _ScanDensity - t * _ScanSpeed) * (2.0 * PI));

                float bandCenter = frac(t * _BandSpeed);
                float band       = SmoothBand(vcoord, bandCenter, max(_BandWidth, 1e-4));

                float rim = pow(saturate(1.0 - abs(dot(N, V))), _RimPower);

                float arcsMod = arcs * (0.65 + 0.35 * scan) * (0.75 + 0.25 * noiseR);
                float bandMod = band;

                float3 color =
                      _RimColor.rgb  * rim
                    + _ArcColor.rgb  * arcsMod
                    + _BaseColor.rgb * bandMod;

                color *= _GlowIntensity;

                float alpha = saturate(_Opacity * (0.35 * rim + 0.6 * arcsMod + 0.9 * bandMod));
                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }

    // If the project is actually not using URP, this fallback avoids magenta but won’t render the effect in Built-in.
    FallBack "Hidden/InternalErrorShader"
}
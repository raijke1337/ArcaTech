Shader "URP/FX/EMP_Shock_Sparks"
{
    Properties
    {
        // Spark texture: use your single-channel (R) spark sheet, set to Repeat
        _SparkTex("Spark Texture (R)", 2D) = "black" {}
        [HDR]_SparkColor("Spark Color (HDR)", Color) = (0.2, 0.85, 1.0, 1.0)
        _SparkIntensity("Spark Intensity", Range(0, 10)) = 3.0

        // How many spark tiles around circumference and along height
        _SparkTilingU("Tiling Around (lanes)", Range(1, 64)) = 16
        _SparkTilingV("Tiling Up (rows)", Range(1, 16)) = 4

        // Movement
        _SparkScrollU("Scroll Around Speed", Range(-10, 10)) = 1.0
        _SparkScrollV("Scroll Up Speed", Range(-10, 10)) = 0.8

        // Spawn and lifetime shaping
        _SparkSpawnRate("Spawn Rate (per sec)", Range(0.1, 10)) = 2.0
        _SparkLifeTravel("Lifetime Travel (rows)", Range(0, 2)) = 0.85

        // Cut black background from spark texture (0..1 threshold)
        _SparkCutoff("Spark Cutoff", Range(0, 1)) = 0.25

        // Optional distortion using a noise texture
        _NoiseTex("Noise (R)", 2D) = "gray" {}
        _NoiseScale("Noise Scale (Angle, Height)", Vector) = (4, 2, 0, 0)
        _SparkWarp("Noise Warp Amount", Range(0, 1)) = 0.2
        _NoiseStrength("Noise Strength (misc)", Range(0, 1)) = 0.35

        // Readability rim
        [HDR]_RimColor("Rim Glow Color", Color) = (0.3, 0.9, 1.0, 1.0)
        _RimStrength("Rim Strength", Range(0, 4)) = 0.6
        _RimPower("Rim Power", Range(0.5, 8.0)) = 2.5

        // Global transparency
        _Opacity("Overall Opacity", Range(0, 1)) = 0.9
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
            Name "EMPShockSparks"
            Tags { "LightMode"="SRPDefaultUnlit" }

            // Soft additive. For full additive: Blend One One
            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SparkTex); SAMPLER(sampler_SparkTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _SparkColor;
                float  _SparkIntensity;
                float  _SparkTilingU;
                float  _SparkTilingV;
                float  _SparkScrollU;
                float  _SparkScrollV;
                float  _SparkSpawnRate;
                float  _SparkLifeTravel;
                float  _SparkCutoff;

                float2 _NoiseScale;
                float  _SparkWarp;
                float  _NoiseStrength;

                float4 _RimColor;
                float  _RimStrength;
                float  _RimPower;

                float  _Opacity;
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

            // Angle around cylinder from object-space xz -> [0,1)
            float Angle01FromXZ(float2 xz)
            {
                float a = atan2(xz.x, xz.y);          // [-PI, PI]
                a = a / (2.0 * PI) + 0.5;             // [0,1)
                return frac(a);
            }

            // Simple stable hash to seed per-tile timing
            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = SafeNormalize(IN.normalWS);
                float3 V = SafeNormalize(_WorldSpaceCameraPos.xyz - IN.positionWS);

                float t = _Time.x;
                float ang01 = Angle01FromXZ(IN.positionOS.xz);   // 0..1 around Y
                float vcoord = saturate(IN.uv.y);                // 0..1 bottom->top

                // Optional noise warp in angle/height space
                float2 nUV = float2(ang01 * _NoiseScale.x, vcoord * _NoiseScale.y + t * 0.25);
                float noiseR = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, nUV).r; // 0..1
                float warp = (noiseR * 2.0 - 1.0) * _SparkWarp;

                // Tile indices for stable per-cell randomization
                float uCell = floor(ang01 * _SparkTilingU);
                float vCell = floor(vcoord * _SparkTilingV);
                float seed  = Hash11(uCell + vCell * 57.0);

                // Life 0..1 per tile, repeats with _SparkSpawnRate
                float life    = frac(t * _SparkSpawnRate + seed);
                float lifeIn  = smoothstep(0.00, 0.15, life);
                float lifeOut = smoothstep(1.00, 0.85, life);
                float lifeFade = lifeIn * lifeOut;

                // Spark UVs: tile, scroll, and per-life travel
                float u = frac(ang01 * _SparkTilingU + t * _SparkScrollU + warp);
                float v = frac(vcoord * _SparkTilingV - t * _SparkScrollV - life * _SparkLifeTravel);

                float2 sparkUV = float2(u, v);
                float sparkTex = SAMPLE_TEXTURE2D(_SparkTex, sampler_SparkTex, sparkUV).r;

                // Cut out black background, then apply fade
                float sparkMask = saturate((sparkTex - _SparkCutoff) / max(1e-4, 1.0 - _SparkCutoff));
                float spark     = sparkMask * lifeFade;

                // Subtle readability rim (top-down)
                float rim = pow(saturate(1.0 - abs(dot(N, V))), _RimPower);

                // Color and alpha (soft additive output uses color * alpha)
                float3 color =
                    _SparkColor.rgb * (spark * _SparkIntensity) +
                    _RimColor.rgb   * (_RimStrength * rim);

                float alpha = saturate(_Opacity * (spark + 0.25 * _RimStrength * rim));

                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
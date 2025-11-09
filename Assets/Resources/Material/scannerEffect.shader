Shader "URP/SonarSpherePulse"
{
    Properties
    {
        _Color("Ring Color", Color) = (0.2, 0.9, 1.0, 1.0)
        _Intensity("Intensity", Range(0,10)) = 2.0

        _SonarRadius("Ring Width (world m)", Range(0.005, 0.6)) = 0.12
        _Feather("Edge Feather", Range(0.0, 1.0)) = 0.35

        _RippleAmp("Ripple Amplitude", Range(0, 1)) = 0.25
        _RippleFreq("Ripple Frequency", Range(0, 50)) = 12.0
        _RippleScroll("Ripple Scroll Speed", Range(-5, 5)) = 0.6

        _GlowBoost("Glow Boost", Range(0, 5)) = 1.2

        // Native radius of your sphere mesh in object space (Unity Sphere = 0.5)
        _MeshRadius("Mesh Radius (object units)", Range(0.1, 1.0)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Front
        ZWrite Off
        ZTest Always
        Blend One One

        Pass
        {
            Name "SonarPulse"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _Intensity;

                float  _RingWidthWS;
                float  _Feather;

                float  _RippleAmp;
                float  _RippleFreq;
                float  _RippleScroll;

                float  _GlowBoost;
                float  _MeshRadius;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 world = TransformObjectToWorld(v.positionOS);
                o.positionCS = TransformWorldToHClip(world);
                o.screenPos  = ComputeScreenPos(o.positionCS);
                return o;
            }

            // Helper to get max axis scale from object-to-world (handles non-uniform scale)
            float MaxAxisScale(float4x4 m)
            {
                float sx = length(float3(m._m00, m._m10, m._m20));
                float sy = length(float3(m._m01, m._m11, m._m21));
                float sz = length(float3(m._m02, m._m12, m._m22));
                return max(sx, max(sy, sz));
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Screen UV
                float2 uv = i.screenPos.xy / i.screenPos.w;

                // 1) Sample scene depth and reconstruct scene world position using URP’s helper
                // This handles perspective/orthographic and reversed-Z automatically.
                float deviceDepth = SampleSceneDepth(uv);
                float3 sceneWS    = ComputeWorldSpacePosition(uv, deviceDepth, UNITY_MATRIX_I_VP);

                // 2) Sonar sphere center and world radius from transform
                float4x4 M = GetObjectToWorldMatrix();
                float3 centerWS = float3(M._m03, M._m13, M._m23);
                float  radiusWS = _MeshRadius * MaxAxisScale(M);

                // 3) World-space ring: soft band where distance ≈ radius
                float dist = distance(sceneWS, centerWS);
                float d    = abs(dist - radiusWS);

                float halfW = max(1e-5, _RingWidthWS * 0.5);
                float inner = halfW * (1.0 - _Feather);
                float outer = halfW * (1.0 + _Feather);
                float ring  = smoothstep(outer, inner, d);
                if (ring <= 0.001) discard;

                // 4) Angular ripple in XZ plane
                float2 dir2     = normalize(sceneWS.xz - centerWS.xz);
                float  angle    = atan2(dir2.y, dir2.x);
                float  ripple   = 0.5 + 0.5 * sin(angle * _RippleFreq + _Time.y * _RippleScroll * 6.2831853);
                float  rippleMod = lerp(1.0, 1.0 + _RippleAmp, ripple);

                // 5) Glow peak centered on the ring
                float centerPeak = exp(-pow(d / max(1e-4, halfW), 2.0)) * _GlowBoost;

                float a = ring;
                float3 col = _Color.rgb * (_Intensity * rippleMod) + _Color.rgb * centerPeak;
                //return half4(1,0,0,0.2);
                return half4(col, a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
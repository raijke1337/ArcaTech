Shader "ARCA/HiddenGlitch"
{
    Properties
    {
        _ContourColor ("Contour Color", Color) = (1.0, 0.31, 0.64, 1) // #FF4FA3
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _FlickerSpeed ("Flicker Speed", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "ForwardUnlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ContourColor;
                float _GlitchIntensity;
                float _FlickerSpeed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = input.uv;
                return output;
            }

            // Простой псевдослучайный шум
            float random(float2 st) { return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123); }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Жесткий Fresnel для эффекта "только контуры"
                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                float rim = 1.0 - saturate(dot(viewDir, input.normalWS));
                rim = pow(rim, 4.0); // Узкая обводка

                // 2. Глитч-смещение
                float noise = random(input.uv * 10.0 + _Time.y);
                float glitch = step(1.0 - _GlitchIntensity, noise);
                
                // 3. Мерцание (иногда полностью пропадает)
                float flicker = step(0.2, sin(_Time.y * _FlickerSpeed) * 0.5 + 0.5);

                float finalAlpha = rim * glitch * flicker * 0.6; // Максимальная прозрачность 60%
                
                return half4(_ContourColor.rgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
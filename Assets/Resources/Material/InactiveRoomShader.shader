Shader "ARCA/InactiveDither"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.13, 0.18, 1) // #1B2230
        _ScanColor ("Scan Color", Color) = (0.16, 0.84, 1.0, 1) // #29D7FF
        _GridColor ("Grid Color", Color) = (0.15, 0.19, 0.27, 1) // #273044
        
        _ScanSpeed ("Scan Speed", Float) = 0.5
        _GridSize ("Grid Size", Float) = 1.0
        _DitherScale ("Dither Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }
        LOD 100
        AlphaToMask On // Ключ к чистому дизерингу без сортировочных артефактов

        Pass
        {
            Name "Forward"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ScanColor;
                float4 _GridColor;
                float _ScanSpeed;
                float _GridSize;
                float _DitherScale;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            float GetDither(float2 uv, float threshold)
            {
                int x = int(uv.x) % 4; int y = int(uv.y) % 4; int index = x + y * 4;
                float bayer[16] = { 0.0/16.0, 8.0/16.0, 2.0/16.0, 10.0/16.0, 12.0/16.0, 4.0/16.0, 14.0/16.0, 6.0/16.0, 3.0/16.0, 11.0/16.0, 1.0/16.0, 9.0/16.0, 15.0/16.0, 7.0/16.0, 13.0/16.0, 5.0/16.0 };
                return threshold - bayer[index];
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Сетка (World Space)
                float2 gridUV = frac(input.positionWS.xz * _GridSize);
                float gridLine = step(0.95, gridUV.x) + step(0.95, gridUV.y);
                float3 gridColor = lerp(_BaseColor.rgb, _GridColor.rgb, saturate(gridLine * 2.0));

                // 2. Бегущий сканер (по оси Y, привязан к мировым координатам)
                float scanPos = frac(_Time.y * _ScanSpeed + input.positionWS.y * 0.5);
                float scanLine = smoothstep(0.45, 0.5, scanPos) * smoothstep(0.55, 0.5, scanPos);
                
                // 3. Комбинирование
                float3 finalColor = lerp(gridColor, _ScanColor.rgb, scanLine * 0.8);
                
                // 4. Dithering прозрачность (базовая прозрачность ~30%, сканер ~70%)
                float baseAlpha = 0.3 + (scanLine * 0.4);
                float2 ditherUV = input.positionCS.xy * _DitherScale;
                clip(GetDither(ditherUV, baseAlpha));

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
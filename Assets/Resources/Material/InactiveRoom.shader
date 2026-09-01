Shader "Arcatech/Room/InactiveScan"
{
    Properties
    {
        [Header(Scan Appearance)]
        _BaseColor ("Hologram Tint", Color) = (0.2, 0.5, 1.0, 1)
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        _ScanlineFreq ("Scanline Frequency", Float) = 40.0
        _NoiseIntensity ("Static Noise", Range(0, 1)) = 0.3
        _FresnelPower ("Edge Glow Power", Float) = 3.0
        _Alpha ("Overall Transparency", Range(0, 1)) = 0.6
        
        [Header(Meta Glitch)]
        _GlitchOffset ("Glitch UV Offset", Range(0, 0.1)) = 0.02
        _GlitchSpeed ("Glitch Trigger Speed", Float) = 5.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            Name "InactiveRoomPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _ScanlineSpeed;
                float _ScanlineFreq;
                float _NoiseIntensity;
                float _FresnelPower;
                float _Alpha;
                float _GlitchOffset;
                float _GlitchSpeed;
            CBUFFER_END
            
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(input.positionOS.xyz));
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Френель для эффекта "обводки" голограммы
                float fresnel = pow(1.0 - saturate(dot(input.normalWS, input.viewDirWS)), _FresnelPower);
                
                // Сканлайны (горизонтальные полосы)
                float scanline = sin(input.worldPos.y * _ScanlineFreq + _Time.y * _ScanlineSpeed);
                scanline = smoothstep(0.3, 0.7, scanline) * 0.5 + 0.5;
                
                // Статический шум
                float noise = Hash(input.uv * 100.0 + _Time.y * 0.5);
                noise = step(0.8, noise) * _NoiseIntensity;
                
                // Мета-глитч: периодическое смещение UV
                float glitchTrigger = step(0.95, frac(_Time.y * _GlitchSpeed));
                float2 glitchUV = input.uv + glitchTrigger * float2(_GlitchOffset, 0);
                
                // Сборка цвета
                half3 color = _BaseColor.rgb;
                color += fresnel * _BaseColor.rgb * 1.5; // Усиление на краях
                color *= scanline;
                color += noise;
                
                // Альфа зависит от френеля и базовой прозрачности
                float alpha = _Alpha * (0.3 + fresnel * 0.7 + scanline * 0.2);
                alpha = saturate(alpha);
                
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
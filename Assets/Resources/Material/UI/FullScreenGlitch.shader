Shader "Arcatech/FullScreenGlitch"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 0
        _RGBSplit ("RGB Split", Range(0, 0.05)) = 0.01
        _BlockSize ("Block Size", Range(1, 100)) = 30
        _BlockAmount ("Block Amount", Range(0, 1)) = 0.5
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.2
        _Speed ("Speed", Float) = 10
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "GlitchPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"


            float _Intensity;
            float _RGBSplit;
            float _BlockSize;
            float _BlockAmount;
            float _ScanlineIntensity;
            float _NoiseIntensity;
            float _Speed;

            SAMPLER(sampler_linear_clamp);
            
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(Hash(i), Hash(i + float2(1,0)), f.x),
                    lerp(Hash(i + float2(0,1)), Hash(i + float2(1,1)), f.x),
                    f.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float time = _Time.y * _Speed;

                // Эффект выключен — просто пропускаем кадр
if (_Intensity < 0.001)
    return SAMPLE_TEXTURE2D(_BlitTexture, sampler_linear_clamp, uv);

                // Блочный глитч
                float blockY = floor(uv.y * _BlockSize);
                if (Hash(float2(blockY, floor(time * 8.0))) > (1.0 - _BlockAmount * _Intensity))
                    uv.x += (Hash(float2(blockY, time)) - 0.5) * 0.2 * _Intensity;

                // Волновой сдвиг
                uv.x += sin(uv.y * 80.0 + time * 15.0) * 0.003 * _Intensity;

                // RGB Split
                float split = _RGBSplit * _Intensity;
half r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_linear_clamp, uv + float2(split, 0)).r;
half g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_linear_clamp, uv).g;
half b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_linear_clamp, uv - float2(split, 0)).b;
                half3 col = half3(r, g, b);

                // Сканлайны
                float scanline = sin(uv.y * 600.0 - time * 3.0) * 0.5 + 0.5;
                col *= 1.0 - _ScanlineIntensity * scanline * _Intensity;

                // Шум
                col += (Noise(uv * 800.0 + time * 50.0) - 0.5) * _NoiseIntensity * _Intensity;

                // Вспышки
                col += step(0.97, Hash(float2(floor(time * 12.0), 0))) * 0.25 * _Intensity;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
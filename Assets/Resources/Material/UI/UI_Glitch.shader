Shader "Arcatech/UIGlitch"
{
    Properties
    {
        [HideInInspector] _MainTex ("Source", 2D) = "white" {}
        
        [Header(Glitch Parameters)]
        _Intensity ("Intensity (0..1)", Range(0, 1)) = 0
        _Speed ("Animation Speed", Float) = 8.0
        _BlockCount ("Horizontal Block Count", Range(4, 64)) = 24
        _ChromaticAberration ("RGB Split (px)", Range(0, 20)) = 6.0
        _ScanlineJitter ("Scanline Jitter", Range(0, 1)) = 0.3
        
        // Настройка снижения движения: при 1 отключает анимацию и смещения
        _ReducedMotion ("Reduced Motion (0/1)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Overlay+100"
            "RenderType" = "Transparent"
        }

        Cull Off ZWrite Off ZTest Always Blend Off

        Pass
        {
            Name "UIGlitch"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _ _REDUCED_MOTION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float  _Intensity;
                float  _Speed;
                float  _BlockCount;
                float  _ChromaticAberration;
                float  _ScanlineJitter;
                float  _ReducedMotion;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            // Псевдослучайный хеш для детерминированного «шума»
            float Hash(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // При снижении движения или нулевой интенсивности — чистая картинка
                if (_ReducedMotion > 0.5 || _Intensity <= 0.001)
                    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float2 uv = input.uv;
                float t = _Time.y * _Speed;

                // --- 1. Блочное смещение строк (горизонтальный сдвиг целых полос) ---
                float blockY = floor(uv.y * _BlockCount);
                float blockRand = Hash((float)(blockY + floor(t * 3.0)));
                // Смещаем только случайные блоки, вероятность зависит от Intensity
                float shiftMask = step(1.0 - _Intensity * 0.7, blockRand);
                float shiftAmount = (Hash((float)(blockY + floor(t * 7.0))) - 0.5) * 0.08 * _Intensity;
                uv.x += shiftAmount * shiftMask;

                // --- 2. Дрожание сканлайнов (тонкие горизонтальные полосы) ---
                float scanline = sin(uv.y * 800.0 + t * 20.0) * 0.5 + 0.5;
                float jitterLine = step(0.97 - _ScanlineJitter * _Intensity * 0.1, scanline);
                uv.x += jitterLine * (Hash(uv.y + t) - 0.5) * 0.03 * _Intensity;

                // --- 3. Хроматическая аберрация (RGB split) ---
                float2 aberration = float2(_ChromaticAberration * _Intensity * _MainTex_TexelSize.x, 0);
                
                half  r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + aberration).r;
                half  g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).g;
                half  b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - aberration).b;
                half  a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;

                // --- 4. Легкое мерцание яркости (имитация нестабильной проекции) ---
                float flicker = 1.0 - Hash((float)(floor(t * 12.0))) * 0.08 * _Intensity;

                return half4(r, g, b, a) * flicker;
            }
            ENDHLSL
        }
    }
}
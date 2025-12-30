Shader "Custom/SpriteWithGlowOutline"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 1.0)) = 0.05  // Now 0-1 range for intuitive control
        _OutlineSoftness ("Outline Softness", Range(0.0, 1.0)) = 0.2   // New: Feathers the edge for better glow
        _GlowIntensity ("Glow Intensity", Range(0.0, 5.0)) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
        Pass
        {
            Name "Unlit"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _OutlineSoftness;
                float _GlowIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Main texture sample
                half4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half mainAlpha = mainColor.a;
                
                // Directions for sampling (8 for smooth coverage)
                static const int numSamples = 8;
                float2 directions[8] = {
                    float2(0.0, 1.0),
                    float2(0.0, -1.0),
                    float2(-1.0, 0.0),
                    float2(1.0, 0.0),
                    float2(0.707, 0.707),
                    float2(0.707, -0.707),
                    float2(-0.707, 0.707),
                    float2(-0.707, -0.707)
                };
                
                // Accumulate outline alpha by sampling at increasing distances
                half maxOutlineAlpha = 0.0;
                half outlineContribution = 0.0;
                
                for (int i = 0; i < numSamples; i++)
                {
                    for (half j = 1.0; j <= 3.0; j++)  // Multi-step for thickness (adjust 3.0 for max steps)
                    {
                        half dist = _OutlineThickness * (j / 3.0);  // Scale distance up to thickness
                        float2 offset = directions[i] * dist;
                        half sampleAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + offset).a;
                        maxOutlineAlpha = max(maxOutlineAlpha, sampleAlpha);
                    }
                }
                
                // Compute outline mask: Where outline alpha exists but main doesn't, softened
                half outlineMask = saturate(maxOutlineAlpha - mainAlpha);
                outlineMask = smoothstep(0.0, _OutlineSoftness, outlineMask);  // Soften edges
                outlineMask *= _GlowIntensity;
                
                // Combine
                half3 finalColor = mainColor.rgb + (_OutlineColor.rgb * outlineMask);
                half finalAlpha = max(mainAlpha, outlineMask * _OutlineColor.a);  // Blend alphas
                
                return half4(finalColor, finalAlpha);
            }
            
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
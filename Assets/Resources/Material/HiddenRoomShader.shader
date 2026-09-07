Shader "ARCA/HiddenGlitch"
{
    Properties
    {
        [Header(Contour)]
        _ContourColor ("Color (Резонанс #FF4FA3)", Color) = (1.0, 0.31, 0.64, 1) 
        _RimPower ("Rim Power (Толщина)", Range(1.0, 10.0)) = 4.0
        
        [Header(Glitch)]
        _GlitchIntensity ("Intensity (Интенсивность разрывов)", Range(0, 1)) = 0.5
        _GlitchScale ("Scale (Размер шума)", Float) = 10.0
        _GlitchSpeed ("Speed (Скорость анимации)", Float) = 5.0
        
        [Header(Transparency)]
        _InnerFillAlpha ("Inner Fill (Заполнение объема)", Range(0, 1)) = 0.05
        _MasterAlpha ("Master Alpha (Общая прозрачность)", Range(0, 1)) = 0.6
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
                float _RimPower;
                float _GlitchIntensity;
                float _GlitchScale;
                float _GlitchSpeed;
                float _InnerFillAlpha;
                float _MasterAlpha;
            CBUFFER_END

            struct Attributes 
            { 
                float4 positionOS : POSITION; 
                float3 normalOS : NORMAL; 
                float2 uv : TEXCOORD0; 
            };
            
            struct Varyings 
            { 
                float4 positionCS : SV_POSITION; 
                float3 positionWS : TEXCOORD0; 
                float3 normalWS : TEXCOORD1; 
                float2 uv : TEXCOORD2; 
            };

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

            // Псевдослучайный шум для генерации помех
            float random(float2 st) 
            { 
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123); 
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Расчет Fresnel (контура)
                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                float rim = 1.0 - saturate(dot(viewDir, input.normalWS));
                rim = pow(rim, _RimPower); // Регулируемая толщина обводки

                // 2. Генерация глитч-шума
                // Шум зависит от UV и времени, создавая эффект "цифрового распада"
                float noise = random(input.uv * _GlitchScale + _Time.y * _GlitchSpeed);
                
                // step создает жесткие рваные края: контур виден только там, где шум превышает порог
                float glitchMask = step(1.0 - _GlitchIntensity, noise);
                
                // 3. Формирование итоговой Альфы
                // Контур умножается на маску глитча (создает эффект прерывистого сигнала)
                float contourAlpha = rim * glitchMask;
                
                // Добавляем слабое внутреннее заполнение, чтобы игрок читал габариты объекта
                float fillAlpha = _InnerFillAlpha * (1.0 - rim); 
                
                // Финальная сборка с учетом мастер-прозрачности
                float finalAlpha = (contourAlpha + fillAlpha) * _MasterAlpha;
                
                return half4(_ContourColor.rgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
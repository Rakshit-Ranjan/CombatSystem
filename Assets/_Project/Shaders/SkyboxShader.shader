Shader "Skybox/StylizedGradientURP"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.25,0.5,0.9,1)
        _HorizonColor ("Horizon Color", Color) = (0.7,0.85,1,1)
        _BottomColor ("Bottom Color", Color) = (1,1,1,1)
        _Exponent ("Gradient Power", Range(0.1,5)) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dirWS : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _HorizonColor;
            float4 _BottomColor;
            float _Exponent;

            Varyings vert (Attributes v)
            {
                Varyings o;

                VertexPositionInputs pos = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = pos.positionCS;
                o.dirWS = normalize(pos.positionWS);

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float h = saturate(i.dirWS.y * 0.5 + 0.5);
                h = pow(h, _Exponent);

                float3 col = lerp(_BottomColor.rgb, _HorizonColor.rgb, h);
                col = lerp(col, _TopColor.rgb, h*h);

                return half4(col, 1);
            }

            ENDHLSL
        }
    }
}
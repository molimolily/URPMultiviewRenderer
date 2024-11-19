Shader "Multiview/DebugSH"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SphericalHarmonics.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                float2 staticLightmapUV   : TEXCOORD1;
                float2 dynamicLightmapUV  : TEXCOORD2;
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                float3 normalWS                 : TEXCOORD2;
                float4 positionCS               : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                real4 _SHAr;
                real4 _SHAg;
                real4 _SHAb;
                real4 _SHBr;
                real4 _SHBg;
                real4 _SHBb;
                real4 _SHC;
            CBUFFER_END

            half3 SampleSH(half3 normalWS)
            {
                // LPPV is not supported in Ligthweight Pipeline
                real4 SHCoefficients[7];
                SHCoefficients[0] = unity_SHAr;
                SHCoefficients[1] = unity_SHAg;
                SHCoefficients[2] = unity_SHAb;
                SHCoefficients[3] = unity_SHBr;
                SHCoefficients[4] = unity_SHBg;
                SHCoefficients[5] = unity_SHBb;
                SHCoefficients[6] = unity_SHC;

                return max(half3(0, 0, 0), SampleSH9(SHCoefficients, normalWS));
            }

            Varyings vert (Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.uv = input.texcoord;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.positionCS = vertexInput.positionCS;

                return output;
            }

            void frag (Varyings input, out half4 outColor : SV_Target)
            {
                outColor = half4(SampleSH(input.normalWS), 1.0);
            }
            ENDHLSL
        }
    }
}

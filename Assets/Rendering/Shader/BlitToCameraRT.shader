Shader "Hidden/BlitToCameraRT"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "BlitToCameraRTPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma target 5.0
            #pragma vertex Vert
            #pragma fragment frag
            #pragma require 2darray

            TEXTURE2D_ARRAY(_ColorRTArray);
            SAMPLER(sampler_ColorRTArray);

            // Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl Ç©ÇÁÉRÉsÅ[
            #if SHADER_API_GLES
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #else
            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #endif

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord   : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            #if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
            #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
            #endif

                output.positionCS = pos;
                // output.texcoord   = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
                output.texcoord   = uv;
                return output;
            }

            static int viewCount_x = 2;
            static int viewCount_y = 2;

            int CalculateSegment(float x, int k)
            {
                x = clamp(x, 0.0, 1.0);
                float interval = 1.0 / k;
                int index = int(x / interval);
                return clamp(index, 0, k - 1);
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                int slice_x = CalculateSegment(uv.x, viewCount_x);
                int slice_y = CalculateSegment(uv.y, viewCount_y);
                int slice = slice_x + slice_y * viewCount_x;

                uv.x = (uv.x - slice_x * (1.0 / viewCount_x)) * viewCount_x;
                uv.y = 1.0f - (uv.y - slice_y * (1.0 / viewCount_y)) * viewCount_y;

                half4 color = SAMPLE_TEXTURE2D_ARRAY(_ColorRTArray, sampler_ColorRTArray, uv, slice);

                return color;
            }
            ENDHLSL
        }
    }
}

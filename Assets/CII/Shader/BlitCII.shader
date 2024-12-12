Shader "Merge/BlitCII"
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
            #include "Assets/Rendering/ShaderLibrary/BlitTextureArray.hlsl"

            #pragma vertex Vert
            #pragma fragment frag
            #pragma require 2darray

            int _ElementWidth;
            int _ElementHeight;
            float4 _Offset;

            int CalculateSegment(float x, int k, float min, float max)
            {
                if(x < min || x > max)
					return -1;
                x = (x - min) / (max - min);
                x = clamp(x, 0.0, 1.0);
                float interval = 1.0 / k;
                int index = int(x / interval);
                return clamp(index, 0, k - 1);
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // graphics apiàÀë∂, ç∂â∫Çå¥ì_Ç…
                #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D12)
                uv.y = 1.0 - uv.y;
                #endif
                float2 center = float2(0.5f + _Offset.x - _Offset.z, 0.5f + _Offset.y - _Offset.w);
                float2 elementSize = float2(_ElementWidth / _ScreenParams.x, _ElementHeight / _ScreenParams.y);
                float2 renderArea = float2(_ViewCountX * elementSize.x, _ViewCountY * elementSize.y);
                float2 renderAreaMin = center - renderArea * 0.5;
                float2 renderAreaMax = center + renderArea * 0.5;
                int slice_x = CalculateSegment(uv.x, _ViewCountX, renderAreaMin.x, renderAreaMax.x);
                if (slice_x == -1)
					return float4(0, 0, 0, 1);
                int slice_y = CalculateSegment(uv.y, _ViewCountY, renderAreaMin.y, renderAreaMax.y);
                if (slice_y == -1)
                    return float4(0, 0, 0, 1);
                int slice = slice_x + slice_y * _ViewCountX;

                uv.x = (uv.x - (renderAreaMin.x + slice_x * elementSize.x)) / elementSize.x * _BlitScaleFactor.x;
                uv.y = (uv.y - (renderAreaMin.y + slice_y * elementSize.y)) / elementSize.y * _BlitScaleFactor.y;

                half4 color = SAMPLE_TEXTURE2D_ARRAY(_ColorRTArray, sampler_ColorRTArray, uv, slice);

                return color;
            }
            ENDHLSL
        }
    }
}

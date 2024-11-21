Shader "Merge/TilingRTArray"
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

                int slice_x = CalculateSegment(uv.x, _ViewCountX);
                int slice_y = CalculateSegment(uv.y, _ViewCountY);
                int slice = slice_x + slice_y * _ViewCountX;

                uv.x = (uv.x - slice_x * (1.0 / _ViewCountX)) * _ViewCountX * _BlitScaleFactor.x;
                uv.y = (1.0f - (uv.y - slice_y * (1.0 / _ViewCountY)) * _ViewCountY) * _BlitScaleFactor.y;

                half4 color = SAMPLE_TEXTURE2D_ARRAY(_ColorRTArray, sampler_ColorRTArray, uv, slice);

                return color;
            }
            ENDHLSL
        }
    }
}

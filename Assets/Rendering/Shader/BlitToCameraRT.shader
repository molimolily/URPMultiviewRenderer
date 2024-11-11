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

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f Vert (appdata v)
			{
				v2f o;
                o.vertex = v.vertex;
				return o;
			}

            half4 frag (v2f input) : SV_Target
            {
                float2 uv = input.vertex.xy;
                half4 color;
                if(uv.x < 0.5f)
                {
                    uv.x *= 2.0f;
                    color = SAMPLE_TEXTURE2D_ARRAY(_ColorRTArray, sampler_ColorRTArray, uv, 1);
                }
                else
				{
                    uv.x = (uv.x - 0.5f) * 2.0f;
					color = SAMPLE_TEXTURE2D_ARRAY(_ColorRTArray, sampler_ColorRTArray, uv, 0);
				}
                
                return color;
            }
            ENDHLSL
        }
    }
}

Shader "Hidden/BlitterMat"
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
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            SAMPLER(sampler_BlitTexture);

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.positionCS.xy;
                half4 color;
                if(uv.x < 0.5f)
                {
                    uv.x *= 2.0f;
                    // color = SAMPLE_TEXTURE2D_ARRAY(_ColorRTArray, sampler_ColorRTArray, uv, 1);
                    color = half4(1, 0, 0, 1);
                }
                else
				{
                    uv.x = (uv.x - 0.5f) * 2.0f;
					// color = SAMPLE_TEXTURE2D_ARRAY(_ColorRTArray, sampler_ColorRTArray, uv, 0);
                    color = half4(0, 1, 0, 1);
				}
                
                return color;
            }
            ENDHLSL
        }
    }
}
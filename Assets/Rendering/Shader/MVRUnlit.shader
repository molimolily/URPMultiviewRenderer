Shader "Multiview/MVRUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "LightMode" = "SRPDefaultUnlit" }
        LOD 100

        Pass
        {
            NAME "MULTIVIEW"

            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature MULTIVIEW_PASS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                // #ifdef MULTIVIEW_PASS
                uint instanceID : SV_InstanceID;
                // #endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // #ifdef MULTIVIEW_PASS
                uint rtIndex : SV_RenderTargetArrayIndex;
                // #endif
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // #ifdef MULTIVIEW_PASS
                o.rtIndex = v.instanceID;
                // #endif
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                col *= _Color;

                return col;
            }
            ENDHLSL
        }
    }
}

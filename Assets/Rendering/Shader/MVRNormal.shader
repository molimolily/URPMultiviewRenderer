Shader "Multiview/MVRNormal"
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
            #pragma multi_compile MULTIVIEW_PASS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                #ifdef MULTIVIEW_PASS
                uint instanceID : SV_InstanceID;
                #endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                #ifdef MULTIVIEW_PASS
                uint rtIndex : SV_RenderTargetArrayIndex;
                #endif
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
                o.normal = TransformObjectToWorldNormal(v.normal);
                #ifdef MULTIVIEW_PASS
                o.rtIndex = v.instanceID;
                #endif
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 normalColor = normalize(i.normal) * 0.5 + 0.5;
                return half4(normalColor, 1.0);
            }
            ENDHLSL
        }
    }
}

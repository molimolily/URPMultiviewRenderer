Shader "Multiview/Debug/Normal"
{
    SubShader
    {
        Tags { "LightMode" = "SRPDefaultUnlit" }
        LOD 100

        Pass
        {
            NAME "MULTIVIEW"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile MULTIVIEW_PASS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Rendering/ShaderLibrary/MultiviewCommon.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                MULTIVIEW_VERTEX_INPUT
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                MULTIVIEW_VERTEX_OUTPUT
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.normal = TransformObjectToWorldNormal(v.normal);
                MULTIVIEW_ASSIGN_RTINDEX(o, v)
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

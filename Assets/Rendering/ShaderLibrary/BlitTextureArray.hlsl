#ifndef BLIT_TEXTUREARRAY_INCLUDED
#define BLIT_TEXTUREARRAY_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D_ARRAY(_ColorRTArray);
SAMPLER(sampler_ColorRTArray);

int _ViewCountX;
int _ViewCountY;
float4 _BlitScaleFactor;

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
};
#endif

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};

Varyings Vert(Attributes input)
{
    Varyings output;

#if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
#else
    float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
    float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
#endif

    output.positionCS = pos;
    output.texcoord = uv;
    return output;
}

#endif

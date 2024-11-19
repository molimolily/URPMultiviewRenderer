#ifndef MULTIVIEW_COMMON_INCLUDED
#define MULTIVIEW_COMMON_INCLUDED

#if defined(MULTIVIEW_PASS)
    #define MULTIVIEW_VERTEX_INPUT uint instanceID : SV_InstanceID;
#else
    #define MULTIVIEW_VERTEX_INPUT
#endif

#if defined(MULTIVIEW_PASS)
    #define MULTIVIEW_VERTEX_OUTPUT uint rtIndex : SV_RenderTargetArrayIndex;
#else
    #define MULTIVIEW_VERTEX_OUTPUT
#endif

#if defined(MULTIVIEW_PASS)
    #define MULTIVIEW_ASSIGN_RTINDEX(o, v) o.rtIndex = v.instanceID;
    #define MULTIVIEW_ASSIGN_RTINDEX_ANY(o, value) o.rtIndex = value;
#else
    #define MULTIVIEW_ASSIGN_RTINDEX(o, v)
#endif

#if defined(MULTIVIEW_PASS)
struct PerViewData
{
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
};

StructuredBuffer<PerViewData> _PerViewData;
#endif

#if defined(MULTIVIEW_PASS)
    #define MULTIVIEW_MATRIX_V(_viewIndex) _PerViewData[_viewIndex].viewMatrix
    #define MULTIVIEW_MATRIX_P(_viewIndex) _PerViewData[_viewIndex].projectionMatrix
    #define MULTIVIEW_MATRIX_VP(_viewIndex) mul(_PerViewData[_viewIndex].projectionMatrix, _PerViewData[_viewIndex].viewMatrix)
    #define MULTIVIEW_MATRIX_MVP(_viewIndex) mul(MULTIVIEW_MATRIX_VP(_viewIndex), unity_ObjectToWorld) 
    // #define MULTIVIEW_MATRIX_MVP(_viewIndex) mul(unity_MatrixVP, unity_ObjectToWorld)
#else
    #define MULTIVIEW_MATRIX_V(_viewIndex) unity_MatrixV
    #define MULTIVIEW_MATRIX_P(_viewIndex) OptimizeProjectionMatrix(glstate_matrix_projection)
    #define MULTIVIEW_MATRIX_VP(_viewIndex) unity_MatrixVP
    #define MULTIVIEW_MATRIX_MVP(_viewIndex) mul(unity_MatrixVP, unity_ObjectToWorld)
#endif

#endif // MULTIVIEW_COMMON_INCLUDED

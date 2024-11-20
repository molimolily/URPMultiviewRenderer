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
    #define MULTIVIEW_VIEWID(v) v.instanceID
#else
    #define MULTIVIEW_VIEWID(v) 0
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
    #define MULTIVIEW_MATRIX_V(v) _PerViewData[v.instanceID].viewMatrix
    #define MULTIVIEW_MATRIX_P(v) _PerViewData[v.instanceID].projectionMatrix
    #define MULTIVIEW_MATRIX_VP(v) mul(_PerViewData[v.instanceID].projectionMatrix, _PerViewData[v.instanceID].viewMatrix)
    #define MULTIVIEW_MATRIX_MVP(v) mul(MULTIVIEW_MATRIX_VP(v), unity_ObjectToWorld)
    #define MULTIVIEW_MATRIX_V_ANY(_viewIndex) _PerViewData[_viewIndex].viewMatrix
    #define MULTIVIEW_MATRIX_P_ANY(_viewIndex) _PerViewData[_viewIndex].projectionMatrix
    #define MULTIVIEW_MATRIX_VP_ANY(_viewIndex) mul(_PerViewData[_viewIndex].projectionMatrix, _PerViewData[_viewIndex].viewMatrix)
    #define MULTIVIEW_MATRIX_MVP_ANY(_viewIndex) mul(MULTIVIEW_MATRIX_VP(_viewIndex), unity_ObjectToWorld) 
#else
    #define MULTIVIEW_MATRIX_V(v) unity_MatrixV
    #define MULTIVIEW_MATRIX_P(v) OptimizeProjectionMatrix(glstate_matrix_projection)
    #define MULTIVIEW_MATRIX_VP(v) unity_MatrixVP
    #define MULTIVIEW_MATRIX_MVP(v) mul(unity_MatrixVP, unity_ObjectToWorld)
    #define MULTIVIEW_MATRIX_V_ANY(_viewIndex) MULTIVIEW_MATRIX_V(_viewIndex)
    #define MULTIVIEW_MATRIX_P_ANY(_viewIndex) MULTIVIEW_MATRIX_P(_viewIndex)
    #define MULTIVIEW_MATRIX_VP_ANY(_viewIndex) MULTIVIEW_MATRIX_VP(_viewIndex)
    #define MULTIVIEW_MATRIX_MVP_ANY(_viewIndex) MULTIVIEW_MATRIX_MVP(_viewIndex)
#endif

#endif // MULTIVIEW_COMMON_INCLUDED

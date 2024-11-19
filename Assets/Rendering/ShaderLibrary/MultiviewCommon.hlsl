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

#endif

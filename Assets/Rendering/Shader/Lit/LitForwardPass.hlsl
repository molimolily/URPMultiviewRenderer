#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

// GLES2 has limited amount of interpolators
#if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD1;
#endif

    float3 normalWS                 : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
#else
    half  fogFactor                 : TEXCOORD5;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                : TEXCOORD7;
#endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    #if defined(_NORMALMAP)
    inputData.tangentToWorld = tangentToWorld;
    #endif
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
#else
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#else
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    // inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    // OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    // output.vertexSH = SampleSHVertex(output.normalWS.xyz);
    output.vertexSH = SampleSH(output.normalWS.xyz);
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
    output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    return output;
}

// Used in Standard (Physically Based) shader
void LitPassFragment(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_PARALLAXMAP)
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
#else
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
#endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    // half4 color = UniversalFragmentPBR(inputData, surfaceData);
    // color.rgb = MixFog(color.rgb, inputData.fogCoord);
    // color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

    half4 color = half4(0.0, 0.0, 0.0, 1.0);
    // outColor = color;
    
    // playground
#if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
#else
    bool specularHighlightsOff = false;
    #endif
    
    BRDFData brdfData;
    InitializeBRDFData(surfaceData, brdfData);
    
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    // ここらへんから異なっている (bakedGI)
    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
    
    /*lightingData.mainLightColor = LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                                              mainLight,
                                                              inputData.normalWS, inputData.viewDirectionWS,
                                                              surfaceData.clearCoatMask, specularHighlightsOff);
    */
    half NdotL = saturate(dot(inputData.normalWS, mainLight.direction));
    // color = half4(NdotL, 0.0, 0.0, 1.0);
    half3 lightColor = mainLight.color;
    // color = half4(lightColor, 1.0);
    half lightAttenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    // color = half4(mainLight.distanceAttenuation, 0.0, 0.0, 1.0); // 違う
    // color = half4(mainLight.shadowAttenuation, 0.0, 0.0, 1.0); // 違う
    color = half4(lightAttenuation, 0.0, 0.0, 1.0); // 違う
    half3 radiance = lightColor * (lightAttenuation * NdotL);
    // color = half4(radiance, 1.0); // 違う
    half3 brdf = brdfData.diffuse;
    // color = half4(lightingData.giColor, 1.0);
    // color = half4(lightingData.mainLightColor, 1.0);
    
    // bakedGIの生成 input.vertexSHとinputData.normalWSのどちらかあるいは両方がおかしい, vertexSHはnormalWSに依存してそう, GlobalIlluminationがおかしい
    // half3 debugBakedGI = SampleSHPixel(input.vertexSH, inputData.normalWS);
    color = half4(input.vertexSH, 1.0);
    // color = half4(debugBakedGI, 1.0);
    
    // これらのパラメータが違いそう, SampleSHが悪さをしているかもしれない
    // color = unity_SHAr; // 違う
    // color = unity_SHAg; // 違う
    // color = unity_SHAb; // 違う
    // color = unity_SHBr; // 違う
    // color = unity_SHBg; // 違う
    // color = unity_SHBb; // 違う
    // color = unity_SHC; // 違う
    
    // color = half4(brdfData.albedo, 1.0);
    // color = half4(brdfData.diffuse, 1.0);
    // color = half4(brdfData.specular, 1.0);
    // color = half4(brdfDataClearCoat.reflectivity, 0.0, 0.0, 1.0);
    // color = half4(brdfData.perceptualRoughness, 0.0, 0.0, 1.0);
    // color = half4(brdfData.roughness, 0.0, 0.0, 1.0);
    // color = half4(brdfData.roughness2, 0.0, 0.0, 1.0);
    // color = half4(brdfData.grazingTerm, 0.0, 0.0, 1.0);
    // color = half4(brdfData.normalizationTerm, 0.0, 0.0, 1.0);
    // color = half4(brdfData.roughness2MinusOne, 0.0, 0.0, 1.0);
    
    // color = half4(surfaceData.albedo, 1.0);
    // color = half4(surfaceData.specular, 1.0);
    // color = half4(surfaceData.metallic, 0.0, 0.0, 1.0);
    // color = half4(surfaceData.smoothness, 0.0, 0.0, 1.0);
    // color = half4(surfaceData.emission, 1.0);
    // color = half4(surfaceData.normalTS, 1.0);
    // color = half4(surfaceData.occlusion, 0.0, 0.0, 1.0);
    // color = half4(surfaceData.alpha, 0.0, 0.0, 1.0);
    // color = half4(surfaceData.clearCoatMask, 0.0, 0.0, 1.0);
    // color = half4(surfaceData.clearCoatSmoothness, 0.0, 0.0, 1.0);
    // color = half4(input.positionWS, 1.0);
    // color = inputData.positionCS;
    // color = half4(inputData.normalWS, 1.0);
    // color = half4(inputData.viewDirectionWS, 1.0);
    // color = inputData.shadowCoord; // 違う
    // color = half4(inputData.fogCoord, 0.0, 0.0, 1.0);
    // color = half4(inputData.vertexLighting, 1.0);
    // color = half4(inputData.bakedGI, 1.0); // 違う
    // color = half4(inputData.normalizedScreenSpaceUV, 0.0, 1.0); // 若干暗い
    // color = inputData.shadowMask;
    // color = half4(inputData.tangentToWorld[0], 1.0);
    // color = half4(inputData.tangentToWorld[1], 1.0);
    // color = half4(inputData.tangentToWorld[2], 1.0);
    
    // color = half4(input.uv, 0.0, 1.0);
    // color = half4(input.positionWS, 1.0);
    // color = half4(input.normalWS, 1.0);
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    // color = input.tangentWS // 使えない?
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    // color = input.fogFactorAndVertexLight; // 使えない?
#endif
    // color = half4(input.fogFactor, 0.0, 0.0, 1.0);
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    // color = input.shadowCoord; // 違う
#endif
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    // color = half4(input.viewDirTS, 1.0); // 使えない？
#endif
#if defined(LIGHTMAP_ON)
    // color = half4(input.staticLightmapUV, 0.0, 1.0);
#else
    // color = half4(input.vertexSH, 1.0); // ライトマップがオフの場合はこれ
#endif
#ifdef DYNAMICLIGHTMAP_ON
    // color = half4(input.dynamicLightmapUV, 0.0, 1.0); // 使えない？
#endif
    // color = input.positionCS;
    // color = half4(aoFactor.indirectAmbientOcclusion, 0.0, 0.0, 1.0);
    // color = half4(lightingData.giColor, 1.0);
    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace MVR
{
    /// <summary>
    /// RTHandleSystemのラッパークラス
    /// </summary>
    public class RTArrayHandleSystem
    {
        RTHandleSystem rtHandleSystem;
        int maxWidth;
        int maxHeight;
        int slices;
        Vector4 scaleFator;

        RTHandle rtHandle;

        public Vector4 ScaleFactor => scaleFator;

        public RTArrayHandleSystem()
        {
            rtHandleSystem = new RTHandleSystem();

            maxWidth = 0;
            maxHeight = 0;
        }

        public void Initialize(int width, int height)
        {
            maxWidth = width;
            maxHeight = height;
            scaleFator = Vector4.one;
        }

        private RTHandle GenerateRTHandle(
            int width,
            int height,
            int slices = 1,
            DepthBits depthBufferBits = DepthBits.None,
            GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            TextureDimension dimension = TextureDimension.Tex2D,
            bool enableRandomWrite = false,
            bool useMipMap = false,
            bool autoGenerateMips = true,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0f,
            MSAASamples msaaSamples = MSAASamples.None,
            bool bindTextureMS = false,
            bool useDynamicScale = false,
            RenderTextureMemoryless memoryless = RenderTextureMemoryless.None,
            VRTextureUsage vrUsage = VRTextureUsage.None,
            string name = ""
            )
        {
            rtHandle?.Release();

            maxWidth = width;
            maxHeight = height;
            this.slices = slices;
            scaleFator = Vector4.one;

            return rtHandleSystem.Alloc(
                width,
                height,
                slices,
                depthBufferBits,
                colorFormat,
                filterMode,
                wrapMode,
                dimension,
                enableRandomWrite,
                useMipMap,
                autoGenerateMips,
                isShadowMap,
                anisoLevel,
                mipMapBias,
                msaaSamples,
                bindTextureMS,
                useDynamicScale,
                memoryless,
                vrUsage,
                name
            );
        }

        private void ComputeScaleFactor(int width, int height)
        {
            scaleFator.x = (float)width / maxWidth;
            scaleFator.y = (float)height / maxHeight;
            scaleFator.z = scaleFator.x;
            scaleFator.w = scaleFator.y;
        }

        /// <summary>
        /// Allocate a new fixed sized RTHandle.
        /// </summary>
        /// <param name="rtHandle">RTHandle to allocate.</param>
        /// <param name="width">With of the RTHandle.</param>
        /// <param name="height">Heigh of the RTHandle.</param>
        /// <param name="slices">Number of slices of the RTHandle.</param>
        /// <param name="depthBufferBits">Bit depths of a depth buffer.</param>
        /// <param name="colorFormat">GraphicsFormat of a color buffer.</param>
        /// <param name="filterMode">Filtering mode of the RTHandle.</param>
        /// <param name="wrapMode">Addressing mode of the RTHandle.</param>
        /// <param name="dimension">Texture dimension of the RTHandle.</param>
        /// <param name="enableRandomWrite">Set to true to enable UAV random read writes on the texture.</param>
        /// <param name="useMipMap">Set to true if the texture should have mipmaps.</param>
        /// <param name="autoGenerateMips">Set to true to automatically generate mipmaps.</param>
        /// <param name="isShadowMap">Set to true if the depth buffer should be used as a shadow map.</param>
        /// <param name="anisoLevel">Anisotropic filtering level.</param>
        /// <param name="mipMapBias">Bias applied to mipmaps during filtering.</param>
        /// <param name="msaaSamples">Number of MSAA samples for the RTHandle.</param>
        /// <param name="bindTextureMS">Set to true if the texture needs to be bound as a multisampled texture in the shader.</param>
        /// <param name="useDynamicScale">Set to true to use hardware dynamic scaling.</param>
        /// <param name="memoryless">Use this property to set the render texture memoryless modes.</param>
        /// <param name="vrUsage">Special treatment of the VR eye texture used in stereoscopic rendering.</param>
        /// <param name="name">Name of the RTHandle.</param>
        /// <returns></returns>
        public RTHandle Alloc(
            int width,
            int height,
            int slices = 1,
            DepthBits depthBufferBits = DepthBits.None,
            GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            TextureDimension dimension = TextureDimension.Tex2D,
            bool enableRandomWrite = false,
            bool useMipMap = false,
            bool autoGenerateMips = true,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0f,
            MSAASamples msaaSamples = MSAASamples.None,
            bool bindTextureMS = false,
            bool useDynamicScale = false,
            RenderTextureMemoryless memoryless = RenderTextureMemoryless.None,
            VRTextureUsage vrUsage = VRTextureUsage.None,
            string name = ""
        )
        {
            // RTHandleがnullまたはスライス数が異なる場合は新規生成
            if (rtHandle == null || this.slices != slices)
            {
                rtHandle = GenerateRTHandle(
                    width,
                    height,
                    slices,
                    depthBufferBits,
                    colorFormat,
                    filterMode,
                    wrapMode,
                    dimension,
                    enableRandomWrite,
                    useMipMap,
                    autoGenerateMips,
                    isShadowMap,
                    anisoLevel,
                    mipMapBias,
                    msaaSamples,
                    bindTextureMS,
                    useDynamicScale,
                    memoryless,
                    vrUsage,
                    name
                );
                return rtHandle;
            }
            else if (width <= maxWidth && height <= maxHeight)
            {
                // 最大解像度以下の場合はスケールファクターを計算して終了
                ComputeScaleFactor(width, height);
                return rtHandle;
            }

            rtHandle = GenerateRTHandle(
                width,
                height,
                slices,
                depthBufferBits,
                colorFormat,
                filterMode,
                wrapMode,
                dimension,
                enableRandomWrite,
                useMipMap,
                autoGenerateMips,
                isShadowMap,
                anisoLevel,
                mipMapBias,
                msaaSamples,
                bindTextureMS,
                useDynamicScale,
                memoryless,
                vrUsage,
                name
            );
            return rtHandle;
        }
       

        public void Dispose()
        {
            rtHandle?.Release();
            rtHandleSystem.Dispose();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiviewRenderPass : ScriptableRenderPass
{
    RTHandle colorTarget;
    RTHandle depthTarget;

    Material blitMat;
    
    public MultiviewRenderPass()
    {
        blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/BlitToCameraRT"));
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        // レンダーターゲットの生成
        if (colorTarget == null)
        {
            colorTarget = RTHandles.Alloc(cameraTextureDescriptor.width, cameraTextureDescriptor.height, 1, DepthBits.None, GraphicsFormat.R8G8B8A8_SRGB, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_CameraColorTexture");
        }

        if (depthTarget == null)
        {
            depthTarget = RTHandles.Alloc(cameraTextureDescriptor.width, cameraTextureDescriptor.height, 1, DepthBits.Depth24, GraphicsFormat.D32_SFloat, FilterMode.Point, TextureWrapMode.Clamp, name: "_CameraDepthTexture");
        }

        // レンダーターゲットの設定
        ConfigureTarget(colorTarget, depthTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("MultiviewRenderPass");

        // Rener: colorTarget, depthTargetに描画
        Render(context, ref renderingData);

        // レンダーターゲットの解除
        ResetTarget();

        // Blit
        if (blitMat != null)
        {
            Blitter.BlitCameraTexture(cmd, colorTarget, k_CameraTarget, blitMat, 0);
        }

        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }


    void Render(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // 不透明オブジェクトの描画
        SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

        // スカイボックスの描画
        if (renderingData.cameraData.camera.clearFlags == CameraClearFlags.Skybox)
        {
            context.DrawSkybox(renderingData.cameraData.camera);
        }

        // 透明オブジェクトの描画
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
    }
}

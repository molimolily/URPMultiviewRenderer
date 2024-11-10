using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiviewRenderPass : ScriptableRenderPass
{
    public Color background;
    public MultiviewRenderPass()
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("MultiviewRenderPass");

        // Rener
        Render(context, ref renderingData);

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

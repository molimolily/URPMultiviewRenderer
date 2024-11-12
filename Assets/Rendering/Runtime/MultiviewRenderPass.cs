using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiviewRenderPass : ScriptableRenderPass
{
    // static readonly GlobalKeyword multiview_Keyword = new GlobalKeyword("MULTIVIEW_PASS");

    Vector2Int currentResolution;
    RTHandle colorRtArray;
    RTHandle depthRtArray;


    public RTHandle ColorRTArray => colorRtArray;

    public MultiviewRenderPass()
    {
        
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        Vector2Int resolution = new Vector2Int(cameraTextureDescriptor.width, cameraTextureDescriptor.height);

        // レンダーターゲットがnull、または解像度が変更された場合にレンダーターゲットを確保
        if (colorRtArray != null || depthRtArray != null || currentResolution != resolution)
        {
            // レンダーターゲットの解放
            colorRtArray?.Release();
            depthRtArray?.Release();

            // レンダーターゲットの確保
            colorRtArray = RTHandles.Alloc(
                width: cameraTextureDescriptor.width / 2,
                height: cameraTextureDescriptor.height,
                slices: 2,
                depthBufferBits: DepthBits.None,
                colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                dimension: TextureDimension.Tex2DArray
                );

            depthRtArray = RTHandles.Alloc(
                width: cameraTextureDescriptor.width / 2,
                height: cameraTextureDescriptor.height,
                slices: 2,
                depthBufferBits: DepthBits.Depth32,
                colorFormat: GraphicsFormat.R32_SFloat,
                dimension: TextureDimension.Tex2DArray
                );

            currentResolution = resolution;
        }

        // レンダーターゲットの設定
        ConfigureTarget(colorRtArray, depthRtArray);

        // レンダーターゲットのクリア
        ConfigureClear(ClearFlag.All, Color.clear);

        cmd.EnableShaderKeyword("MULTIVIEW_PASS");
        cmd.SetInstanceMultiplier(2);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("MultiviewRenderPass");

        // Rener: colorTarget, depthTargetに描画
        Render(context, ref renderingData);

        // レンダーターゲットの解除
        // ResetTarget();

        /*using (new ProfilingScope(cmd, new ProfilingSampler("Blit")))
        {
            // Blit
            if (blitMat != null)
            {
                Blitter.BlitCameraTexture(cmd, colorTarget, k_CameraTarget, blitMat, 0);
            }
            blitMat.SetTexture("_ColorRTArray", colorRtArray);
            cmd.DrawProcedural(Matrix4x4.identity, blitMat, 0, MeshTopology.Quads, 4, 1);
        }*/

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
        drawingSettings.SetShaderPassName(1, new ShaderTagId("UniversalForward"));
        drawingSettings.enableInstancing = true;
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

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);

        cmd.DisableShaderKeyword("MULTIVIEW_PASS");
        cmd.SetInstanceMultiplier(1);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);

    }
}

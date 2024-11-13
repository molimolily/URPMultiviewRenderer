using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TilingPass : ScriptableRenderPass
{
    Material tilingMaterial;
    RTHandle colorRTArray;

    public TilingPass(Material tilingMaterial)
    {
        this.tilingMaterial = tilingMaterial;
        renderPassEvent = RenderPassEvent.AfterRendering;
        ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public void SetInput(RTHandle colorRTArray)
    {
        this.colorRTArray = colorRTArray;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("TilingPass");

        // インスタンス数のリセット
        cmd.SetInstanceMultiplier(1);

        // カメラのビューポートを設定
        Rect camRect = renderingData.cameraData.camera.pixelRect;
        cmd.SetViewport(camRect);

        // TextureArrayのタイリング
        if (tilingMaterial != null && colorRTArray != null)
        {
            tilingMaterial.SetTexture("_ColorRTArray", colorRTArray.rt);
            cmd.DrawProcedural(Matrix4x4.identity, tilingMaterial, 0, MeshTopology.Triangles, 3, 1);
        }

        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }
}

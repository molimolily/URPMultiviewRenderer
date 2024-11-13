using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CombineRTArrayPass : ScriptableRenderPass
{
    Material blitMat;
    RTHandle colorRTArray;

    public CombineRTArrayPass(Material blitMaterial)
    {
        blitMat = blitMaterial;
        renderPassEvent = RenderPassEvent.AfterRendering;
        ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public void SetInput(RTHandle colorRTArray)
    {
        this.colorRTArray = colorRTArray;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("CombineRTArrayPass");

        // インスタンス数のリセット
        cmd.SetInstanceMultiplier(1);

        // カメラのビューポートを設定
        Rect camRect = renderingData.cameraData.camera.pixelRect;
        cmd.SetViewport(camRect);

        // Blit処理
        if (blitMat != null && colorRTArray != null)
        {
            blitMat.SetTexture("_ColorRTArray", colorRTArray.rt);
            cmd.DrawProcedural(Matrix4x4.identity, blitMat, 0, MeshTopology.Triangles, 3, 1);
        }

        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }
}

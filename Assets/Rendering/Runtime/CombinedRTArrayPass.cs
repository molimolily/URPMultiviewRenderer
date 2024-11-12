using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CombinedRTArrayPass : ScriptableRenderPass
{
    Material blitMat;
    RTHandle colorRTArray;

    public RTHandle ColorRTArray { set => colorRTArray = value; }

    public CombinedRTArrayPass(Material blitMaterial)
    {
        blitMat = blitMaterial;
        renderPassEvent = RenderPassEvent.AfterRendering;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {

    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("CombinedRTArrayPass");

        if (blitMat != null && colorRTArray != null)
        {
            blitMat.SetTexture("_ColorRTArray", colorRTArray.rt);
        }

        cmd.DrawProcedural(Matrix4x4.identity, blitMat, 0, MeshTopology.Triangles, 3, 1);

        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }
}

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

        // Clear the screen
        cmd.ClearRenderTarget(true, true, background);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    void RenderOpaques()
    {

    }
}

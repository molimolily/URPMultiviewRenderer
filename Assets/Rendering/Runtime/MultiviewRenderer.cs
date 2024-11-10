using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiviewRenderer : ScriptableRenderer
{
    MultiviewRenderPass multiviewRenderPass;
    MultiviewRendererData rendererData;

    public MultiviewRenderer(ScriptableRendererData data) : base(data)
    {
        multiviewRenderPass = new MultiviewRenderPass();
        rendererData = data as MultiviewRendererData;
    }

    public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        EnqueuePass(multiviewRenderPass);

        multiviewRenderPass.background = rendererData.backgroundColor;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

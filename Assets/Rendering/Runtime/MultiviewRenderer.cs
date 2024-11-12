using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class MultiviewRenderer : ScriptableRenderer
{
    MultiviewRendererData rendererData;
    MultiviewRenderPass multiviewRenderPass;
    CombinedRTArrayPass combinedRTArrayPass;

    Material blitMat;

    public MultiviewRenderer(ScriptableRendererData data) : base(data)
    {
        rendererData = data as MultiviewRendererData;
        multiviewRenderPass = new MultiviewRenderPass();

        blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/BlitToCameraRT"));
        // blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/BlitterMat"));
        combinedRTArrayPass = new CombinedRTArrayPass(blitMat);
    }

    public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        EnqueuePass(multiviewRenderPass);

        combinedRTArrayPass.ColorRTArray = multiviewRenderPass.ColorRTArray;

        EnqueuePass(combinedRTArrayPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(blitMat);
    }
}

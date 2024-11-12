using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/*public class FinalBlitPass : ScriptableRenderPass
{
    Material blitMat;
    RTHandle colorRTArray;

    public RTHandle ColorRTArray { set => colorRTArray = value; }
    public FinalBlitPass(Material mat)
    {
        blitMat = mat;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        // ConfigureTarget(k_CameraTarget);
        // ResetTarget();
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("FinalBlitPass");

        if(blitMat != null && colorRTArray != null)
        {
            // blitMat.SetTexture("_ColorRTArray", colorRTArray);
            Debug.Log("colorRTArray: " + colorRTArray.name);
            Debug.Log("colorRTArray: " + colorRTArray.ToString());
        }
        
        cmd.DrawProcedural(Matrix4x4.identity, blitMat, 0, MeshTopology.Quads, 4, 1);
        // cmd.DrawProcedural(Matrix4x4.identity, blitMat, 0, MeshTopology.Triangles, 3, 1);

        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }
}
*/
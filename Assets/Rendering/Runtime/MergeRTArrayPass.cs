using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    public class MergeRTArrayPass : ScriptableRenderPass
    {
        Material mergeMaterial;
        RTHandle colorRTArray;

        public MergeRTArrayPass(Material mergeMaterial)
        {
            this.mergeMaterial = mergeMaterial;
            renderPassEvent = RenderPassEvent.AfterRendering;
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public void SetInput(RTHandle colorRTArray)
        {
            this.colorRTArray = colorRTArray;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("MergeRTArrayPass");

            // インスタンス数のリセット
            cmd.SetInstanceMultiplier(1);

            // カメラのビューポートを設定
            Rect camRect = renderingData.cameraData.camera.pixelRect;
            cmd.SetViewport(camRect);

            // TextureArrayのタイリング
            if (mergeMaterial != null && colorRTArray != null)
            {
                mergeMaterial.SetTexture("_ColorRTArray", colorRTArray.rt);
                cmd.DrawProcedural(Matrix4x4.identity, mergeMaterial, 0, MeshTopology.Triangles, 3, 1);
            }

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }
    }
}


using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    public class MergeRTArrayPass : ScriptableRenderPass
    {
        Material mergeMaterial;
        Vector4 scaleFactor;
        RTHandle colorRTArray;

        public MergeRTArrayPass(Material mergeMaterial)
        {
            this.mergeMaterial = mergeMaterial;
            renderPassEvent = RenderPassEvent.AfterRendering;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureInput(ScriptableRenderPassInput.Color);
            ConfigureTarget(k_CameraTarget);
            ConfigureClear(ClearFlag.Color, Color.clear);
        }

        public void SetInput(RTHandle colorRTArray, Vector4 scaleFactor)
        {
            this.colorRTArray = colorRTArray;
            this.scaleFactor = scaleFactor;
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
                mergeMaterial.SetVector("_BlitScaleFactor", scaleFactor);
                CoreUtils.DrawFullScreen(cmd, mergeMaterial);
            }

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }
    }
}


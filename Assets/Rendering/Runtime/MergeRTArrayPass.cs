using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    public class MergeRTArrayPass : ScriptableRenderPass
    {
        Material mergeMaterial;
        RTHandleProperties rtHandleProperties;
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

        public void SetInput(RTHandle colorRTArray, RTHandleProperties rtHandleProperties)
        {
            this.colorRTArray = colorRTArray;
            this.rtHandleProperties = rtHandleProperties;
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
                mergeMaterial.SetVector("_BlitScaleFactor", rtHandleProperties.rtHandleScale);
                CoreUtils.DrawFullScreen(cmd, mergeMaterial);
                // Blitter.BlitCameraTexture(cmd, colorRTArray, k_CameraTarget, mergeMaterial, 0);
            }

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }
    }
}


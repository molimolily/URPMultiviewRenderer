using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{

    public class MultiviewRenderPass : ScriptableRenderPass
    {
        public Vector2Int viewCount;

        RTHandle colorRtArray;
        RTHandle depthRtArray;

        public MultiviewRenderPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // レンダーターゲットの設定
            ConfigureTarget(colorRtArray, depthRtArray);

            // shader keywordの設定
            // cmd.EnableKeyword(multiview_Keyword);
            // cmd.DisableKeyword(multiview_Keyword);

            cmd.SetGlobalInt("_ViewCountX", viewCount.x);
            cmd.SetGlobalInt("_ViewCountY", viewCount.y);

            // 視点数だけインスタンス数を乗算
            cmd.SetInstanceMultiplier((uint)(viewCount.x * viewCount.y));
        }

        public void SetTarget(RTHandle color, RTHandle depth)
        {
            colorRtArray = color;
            depthRtArray = depth;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Clear");

            Camera camera = renderingData.cameraData.camera;

            // レンダーターゲットのクリア
            ClearRenderTarget(cmd, camera);

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);

            Render(context, ref renderingData);
        }

        void ClearRenderTarget(CommandBuffer cmd, Camera camera)
        {
            CameraClearFlags clearFlags = camera.clearFlags;
            cmd.ClearRenderTarget(clearFlags <= CameraClearFlags.Depth,
                clearFlags <= CameraClearFlags.Color,
                clearFlags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        }

        void Render(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 不透明オブジェクトの描画
            SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);
            drawingSettings.SetShaderPassName(1, new ShaderTagId("UniversalForward"));
            drawingSettings.perObjectData = PerObjectData.LightProbe | PerObjectData.LightProbeProxyVolume | PerObjectData.Lightmaps | PerObjectData.LightData | PerObjectData.ReflectionProbes;
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            // スカイボックスの描画
            if (renderingData.cameraData.camera.clearFlags == CameraClearFlags.Skybox)
            {
                context.DrawSkybox(renderingData.cameraData.camera);
            }

            // 透明オブジェクトの描画
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            // cmd.DisableKeyword(multiview_Keyword);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // cmd.DisableKeyword(multiview_Keyword);
            cmd.SetInstanceMultiplier(1);
        }
    }

}
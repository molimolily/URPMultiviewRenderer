using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{

    public class MultiviewRenderPass : ScriptableRenderPass
    {
        public static readonly GlobalKeyword multiview_Keyword = GlobalKeyword.Create("MULTIVIEW_PASS");

        public Vector2Int viewCount;

        RTHandle colorRtArray;
        RTHandle depthRtArray;
        Vector4 scaleFactor;

        public MultiviewRenderPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {

            // レンダーターゲットの設定
            ConfigureTarget(colorRtArray, depthRtArray);
            
            // 視点数だけインスタンス数を乗算
            cmd.SetInstanceMultiplier((uint)(viewCount.x * viewCount.y));
        }

        public void SetTarget(RTHandle color, RTHandle depth, Vector4 scaleFactor)
        {
            colorRtArray = color;
            depthRtArray = depth;
            this.scaleFactor = scaleFactor;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            Camera camera = renderingData.cameraData.camera;

            using (new ProfilingScope(cmd, new ProfilingSampler("MultiviewPass-Setup")))
            {
                // レンダーターゲットのクリア
                ClearRenderTarget(cmd, camera);

                // keywordの有効化
                cmd.SetKeyword(multiview_Keyword, true);

                // カメラのビューポートを設定
                cmd.SetViewport(new Rect(0, 0, Mathf.CeilToInt(colorRtArray.rt.width * scaleFactor.x), Mathf.CeilToInt(colorRtArray.rt.height * scaleFactor.y)));
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            Render(context, ref renderingData);
        }

        void ClearRenderTarget(CommandBuffer cmd, Camera camera)
        {
            float depth = 1.0f; // Depth Bufferのクリア値
            CameraClearFlags clearFlags = camera.clearFlags;
            cmd.ClearRenderTarget(clearFlags <= CameraClearFlags.Depth,
                clearFlags <= CameraClearFlags.Color,
                clearFlags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear,
                depth);
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
            // keywordの無効化
            cmd.SetKeyword(multiview_Keyword, false);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.SetInstanceMultiplier(1);
        }
    }

}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace MVR
{
    public class MultiviewRenderer : ScriptableRenderer
    {
        MultiviewRendererData rendererData;
        MultiviewRenderPass multiviewRenderPass;
        MergeRTArrayPass mergeRTArrayPass;

        Material mergeMaterial;

        Vector2Int currentResolution;

        Dictionary<int, IMultiviewCameraHandler> cameraHandlerCache = new Dictionary<int, IMultiviewCameraHandler>();

        ForwardLights forwardLights;

        public MultiviewRenderer(Shader mergeShader, ScriptableRendererData data) : base(data)
        {
            rendererData = data as MultiviewRendererData;

            // マルチビューレンダーパス
            multiviewRenderPass = new MultiviewRenderPass();

            // マージマテリアルの設定
            if (mergeShader == null)
                mergeShader = Shader.Find("Merge/TilingRTArray");
            mergeMaterial = CoreUtils.CreateEngineMaterial(mergeShader);

            // マージパス
            mergeRTArrayPass = new MergeRTArrayPass(mergeMaterial);

            // ライティングの設定
            forwardLights = new ForwardLights();
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor camTexDesc = cameraData.cameraTargetDescriptor;
            Vector2Int resolution = new Vector2Int(camTexDesc.width, camTexDesc.height);

            int cameraID = cameraData.camera.GetHashCode();
            // カメラごとのハンドラを取得
            if (!cameraHandlerCache.TryGetValue(cameraID, out IMultiviewCameraHandler handler))
            {
                handler = cameraData.camera.GetComponent<IMultiviewCameraHandler>();
                cameraHandlerCache.Add(cameraID, handler);
            }

            // ハンドラがnullの場合はレンダリングを行わない
            if (handler == null)
            {
                Debug.LogWarning("ICameraPayload is not attached to the camera. Rendering is not performed.");
                return;
            }

            // レンダリングの有無
            if (!handler.ShouldRender)
            {
                return;
            }

#if UNITY_EDITOR
            // スライス数と視点数のチェック
            if (handler.ViewCount.x * handler.ViewCount.y > SystemInfo.maxTextureArraySlices)
            {
                Debug.LogWarning("The number of slices exceeds the maximum number of slices supported by the device.");
                return;
            }
#endif

            // スクリーンリサイズ時の処理
            // NOTE: 固定解像度のときは初期化時のみ, aspect比で解像度を指定している場合にリサイズ処理が発生し得る
            if (currentResolution.x != resolution.x || currentResolution.y != resolution.y)
            {
                currentResolution = resolution;
                handler.OnScreenResize(resolution.x, resolution.y);
            }

            // レンダーターゲットのnullチェック
            if (handler.ColorTarget == null || handler.DepthTarget == null)
            {
                handler.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // レンダーターゲットの設定
            multiviewRenderPass.SetTarget(handler.ColorTarget, handler.DepthTarget, handler.ScaleFactor);

            // 視点数の設定
            multiviewRenderPass.viewCount = handler.ViewCount;


            // Global変数の設定
            CommandBuffer cmd = CommandBufferPool.Get("Setup");
            cmd.SetGlobalInt("_ViewCountX", handler.ViewCount.x);
            cmd.SetGlobalInt("_ViewCountY", handler.ViewCount.y);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            // ビューデータの設定
            handler.SetViewData(context, ref renderingData);

            // レンダーテクスチャの設定
            mergeRTArrayPass.SetInput(handler.ColorTarget, handler.ScaleFactor);

            // merge materialのセットアップ
            handler.SetupMergeMaterial(mergeMaterial);

            // passの追加
            EnqueuePass(multiviewRenderPass);
            EnqueuePass(mergeRTArrayPass);
        }

        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            forwardLights.Setup(context, ref renderingData);
        }

        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
        {
            cullingParameters.maximumVisibleLights = UniversalRenderPipeline.maxVisibleAdditionalLights + 1;
            // cullingParameters.shadowDistance = cameraData.maxShadowDistance;
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(mergeMaterial);
        }
    }

}
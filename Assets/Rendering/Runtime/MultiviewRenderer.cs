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

        static readonly int viewMatricesID = Shader.PropertyToID("_Multiview_ViewMatrices");
        static readonly int projectionMatricesID = Shader.PropertyToID("_Multiview_ProjectionMatrices");

        public MultiviewRenderer(Shader mergeShader, ScriptableRendererData data) : base(data)
        {
            rendererData = data as MultiviewRendererData;

            // マルチビューレンダーパス
            multiviewRenderPass = new MultiviewRenderPass();

            // int maxTextureArraySlices = SystemInfo.supports2DArrayTextures ? SystemInfo.maxTextureArraySlices : 0;
            // Debug.Log("Max Texture2DArray Slices: " + maxTextureArraySlices);

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

#if UNITY_EDITOR
            // スライス数と視点数のチェック
            if (handler.ViewCount.x * handler.ViewCount.y > SystemInfo.maxTextureArraySlices)
            {
                Debug.LogWarning("The number of slices exceeds the maximum number of slices supported by the device.");
                return;
            }
#endif

            // レンダーターゲットの生成
            if (handler.ColorTarget == null || handler.DepthTarget == null)
            {
                // Debug.Log("Generate Render Target");
                handler.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // 視点数とスライス数が異なる場合はレンダーターゲットを再確保
            if (handler.ColorTarget.rt.volumeDepth != handler.ViewCount.x * handler.ViewCount.y)
            {
                // Debug.Log("Reallocate Render Target");
                handler.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // スクリーンリサイズ時の処理
            if (currentResolution.x != resolution.x || currentResolution.y != resolution.y)
            {
                // Debug.Log("Screen Resize");
                currentResolution = resolution;
                handler.OnScreenResize(resolution.x, resolution.y);
            }

            // レンダーターゲットの設定
            multiviewRenderPass.SetTarget(handler.ColorTarget, handler.DepthTarget);

            // 視点数の設定
            multiviewRenderPass.viewCount = handler.ViewCount;

            // ビューデータの設定
            handler.SetViewData(context, ref renderingData);

            // レンダーテクスチャの設定
            mergeRTArrayPass.SetInput(handler.ColorTarget);

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
            cullingParameters.shadowDistance = cameraData.maxShadowDistance;
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(mergeMaterial);
        }
    }

}
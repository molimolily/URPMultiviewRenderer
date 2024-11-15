using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    public class MultiviewRenderer : ScriptableRenderer
    {
        MultiviewRendererData rendererData;
        MultiviewRenderPass multiviewRenderPass;
        MergeRTArrayPass mergeRTArrayPass;

        Material mergeMaterial;

        Vector2Int currentResolution;

        Dictionary<Camera, ICameraPayload> payloadCache = new Dictionary<Camera, ICameraPayload>();

        public MultiviewRenderer(Vector2Int viewCount, Vector2Int viewResolution, Shader mergeShader, ScriptableRendererData data) : base(data)
        {
            rendererData = data as MultiviewRendererData;

            // マルチビューレンダーパス
            multiviewRenderPass = new MultiviewRenderPass();

            int maxTextureArraySlices = SystemInfo.supports2DArrayTextures ? SystemInfo.maxTextureArraySlices : 0;
            Debug.Log("Max Texture2DArray Slices: " + maxTextureArraySlices);

            // マージマテリアルの設定
            if (mergeShader == null)
                mergeShader = Shader.Find("Merge/TilingRTArray");
            mergeMaterial = CoreUtils.CreateEngineMaterial(mergeShader);

            // マージパス
            mergeRTArrayPass = new MergeRTArrayPass(mergeMaterial);
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor camTexDesc = cameraData.cameraTargetDescriptor;
            Vector2Int resolution = new Vector2Int(camTexDesc.width, camTexDesc.height);

            // カメラごとのペイロードを取得
            if(!payloadCache.TryGetValue(cameraData.camera, out ICameraPayload payload))
            {
                payload = cameraData.camera.GetComponent<ICameraPayload>();
                payloadCache.Add(cameraData.camera, payload);
            }

            // ペイロードがnullの場合はレンダリングを行わない
            if (payload == null)
            {
                Debug.LogWarning("ICameraPayload is not attached to the camera. Rendering is not performed.");
                return;
            }

#if UNITY_EDITOR
            // スライス数と視点数のチェック
            if (payload.ViewCount.x * payload.ViewCount.y > SystemInfo.maxTextureArraySlices)
            {
                Debug.LogWarning("The number of slices exceeds the maximum number of slices supported by the device.");
                return;
            }
#endif

            // レンダーターゲットの生成
            if (payload.ColorTarget == null || payload.DepthTarget == null)
            {
                Debug.Log("Generate Render Target");
                payload.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // 視点数とスライス数が異なる場合はレンダーターゲットを再確保
            if (payload.ColorTarget.rt.volumeDepth != payload.ViewCount.x * payload.ViewCount.y)
            {
                Debug.Log("Reallocate Render Target");
                payload.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // スクリーンリサイズ時の処理
            if (currentResolution.x != resolution.x || currentResolution.y != resolution.y)
            {
                Debug.Log("Screen Resize");
                currentResolution = resolution;
                payload.OnScreenResize(resolution.x, resolution.y);
            }

            // レンダーターゲットの設定
            multiviewRenderPass.SetTarget(payload.ColorTarget, payload.DepthTarget);

            // 視点数の設定
            multiviewRenderPass.viewCount = payload.ViewCount;

            // レンダーテクスチャの設定
            mergeRTArrayPass.SetInput(payload.ColorTarget);

            // passの追加
            EnqueuePass(multiviewRenderPass);
            EnqueuePass(mergeRTArrayPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(mergeMaterial);
        }
    }

}
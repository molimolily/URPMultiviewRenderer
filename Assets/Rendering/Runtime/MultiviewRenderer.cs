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

        Dictionary<int, ICameraPayload> cameraPayloadCache = new Dictionary<int, ICameraPayload>();

        ForwardLights forwardLights;

        public MultiviewRenderer(Shader mergeShader, ScriptableRendererData data) : base(data)
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

            // ライティングの設定
            forwardLights = new ForwardLights();
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor camTexDesc = cameraData.cameraTargetDescriptor;
            Vector2Int resolution = new Vector2Int(camTexDesc.width, camTexDesc.height);

            int cameraID = cameraData.camera.GetHashCode();
            // カメラごとのペイロードを取得
            if (!cameraPayloadCache.TryGetValue(cameraID, out ICameraPayload payload))
            {
                payload = cameraData.camera.GetComponent<ICameraPayload>();
                cameraPayloadCache.Add(cameraID, payload);
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

            /*SphericalHarmonicsL2 sh = RenderSettings.ambientProbe;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Debug.Log($"SH[{i},{j}] = {sh[i, j]}");
                }
            }*/
        }

        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // forwardLights.Setup(context, ref renderingData);
            // SH係数の設定
            SphericalHarmonicsL2 sh = RenderSettings.ambientProbe;
            Shader.SetGlobalVector("_SHAr", new Vector4(sh[0, 3], sh[0, 1], sh[0, 2], sh[0, 0]));
            Shader.SetGlobalVector("_SHAg", new Vector4(sh[0, 4], sh[0, 5], sh[0, 6], sh[0, 7]));
            Shader.SetGlobalVector("_SHAb", new Vector4(sh[0, 8], sh[1, 0], sh[1, 1], sh[1, 2]));
            Shader.SetGlobalVector("_SHBr", new Vector4(sh[1, 3], sh[1, 4], sh[1, 5], sh[1, 6]));
            Shader.SetGlobalVector("_SHBg", new Vector4(sh[1, 7], sh[1, 8], sh[2, 0], sh[2, 1]));
            Shader.SetGlobalVector("_SHBb", new Vector4(sh[2, 2], sh[2, 3], sh[2, 4], sh[2, 5]));
            Shader.SetGlobalVector("_SHC", new Vector4(sh[2, 6], sh[2, 7], sh[2, 8], 1.0f));
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
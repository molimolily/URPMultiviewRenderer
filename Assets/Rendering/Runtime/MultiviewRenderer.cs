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

        RTHandle colorRTArray;
        RTHandle depthRTArray;
        Vector2Int currentResolution;

        Material mergeMaterial;
        Vector2Int viewCount;
        Vector2Int viewResolution;

        Dictionary<Camera, ICameraPayload> payloadCache = new Dictionary<Camera, ICameraPayload>();

        public MultiviewRenderer(Vector2Int viewCount, Vector2Int viewResolution, Shader mergeShader, ScriptableRendererData data) : base(data)
        {
            rendererData = data as MultiviewRendererData;

            // マルチビューレンダーパス
            multiviewRenderPass = new MultiviewRenderPass();

            // 視点数の設定
            this.viewCount = viewCount;
            multiviewRenderPass.viewCount = viewCount;

            // 視点解像度の設定
            this.viewResolution = viewResolution;

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

            // レンダーターゲットがnull、または解像度が変更された場合にレンダーターゲットを確保
            if (colorRTArray == null || depthRTArray == null || currentResolution != resolution)
            {
                colorRTArray?.Release();
                depthRTArray?.Release();

                colorRTArray = RTHandles.Alloc(
                        width: viewResolution.x,
                        height: viewResolution.y,
                        slices: viewCount.x * viewCount.y,
                        depthBufferBits: DepthBits.None,
                        colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                        filterMode: FilterMode.Bilinear,
                        dimension: TextureDimension.Tex2DArray
                        );

                depthRTArray = RTHandles.Alloc(
                    width: viewResolution.x,
                    height: viewResolution.y,
                    slices: viewCount.x * viewCount.y,
                    depthBufferBits: DepthBits.Depth32,
                    colorFormat: GraphicsFormat.R32_SFloat,
                    filterMode: FilterMode.Point,
                    dimension: TextureDimension.Tex2DArray
                    );

                currentResolution = resolution;
            }

            // レンダーターゲットの設定
            multiviewRenderPass.SetTarget(colorRTArray, depthRTArray);

            // レンダーテクスチャの設定
            mergeRTArrayPass.SetInput(colorRTArray);

            // passの追加
            EnqueuePass(multiviewRenderPass);
            EnqueuePass(mergeRTArrayPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(mergeMaterial);

            colorRTArray?.Release();
            depthRTArray?.Release();
        }
    }

}
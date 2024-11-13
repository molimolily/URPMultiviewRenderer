using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiviewRenderer : ScriptableRenderer
{
    MultiviewRendererData rendererData;
    MultiviewRenderPass multiviewRenderPass;
    TilingPass tilingPass;

    RTHandle colorRTArray;
    RTHandle depthRTArray;
    Vector2Int currentResolution;

    Material tilingMaterial;
    Vector2Int viewCount;

    public MultiviewRenderer(ScriptableRendererData data) : base(data)
    {
        rendererData = data as MultiviewRendererData;
        multiviewRenderPass = new MultiviewRenderPass();

        // 視点数の設定
        viewCount = new Vector2Int(40, 20);
        multiviewRenderPass.viewCount = viewCount;

        int maxTextureArraySlices = SystemInfo.supports2DArrayTextures ? SystemInfo.maxTextureArraySlices: 0;
        Debug.Log("Max Texture2DArray Slices: " + maxTextureArraySlices);


        tilingMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/TilingRTArray"));
        tilingPass = new TilingPass(tilingMaterial);
    }

    public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        ref CameraData cameraData = ref renderingData.cameraData;
        RenderTextureDescriptor camTexDesc = cameraData.cameraTargetDescriptor;
        Vector2Int resolution = new Vector2Int(camTexDesc.width, camTexDesc.height);

        // レンダーターゲットがnull、または解像度が変更された場合にレンダーターゲットを確保
        if (colorRTArray == null || depthRTArray == null || currentResolution != resolution)
        {
            colorRTArray?.Release();
            depthRTArray?.Release();

            colorRTArray = RTHandles.Alloc(
                    width: camTexDesc.width / viewCount.x,
                    height: camTexDesc.height / viewCount.y,
                    slices: viewCount.x * viewCount.y,
                    depthBufferBits: DepthBits.None,
                    colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                    filterMode: FilterMode.Bilinear,
                    dimension: TextureDimension.Tex2DArray
                    );

            depthRTArray = RTHandles.Alloc(
                width: camTexDesc.width / viewCount.x,
                height: camTexDesc.height / viewCount.y,
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
        tilingPass.SetInput(colorRTArray);

        // passの追加
        EnqueuePass(multiviewRenderPass);
        EnqueuePass(tilingPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(tilingMaterial);

        colorRTArray?.Release();
        depthRTArray?.Release();
    }
}

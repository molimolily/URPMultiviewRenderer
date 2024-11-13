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

        // ���_���̐ݒ�
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

        // �����_�[�^�[�Q�b�g��null�A�܂��͉𑜓x���ύX���ꂽ�ꍇ�Ƀ����_�[�^�[�Q�b�g���m��
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

        // �����_�[�^�[�Q�b�g�̐ݒ�
        multiviewRenderPass.SetTarget(colorRTArray, depthRTArray);

        // �����_�[�e�N�X�`���̐ݒ�
        tilingPass.SetInput(colorRTArray);

        // pass�̒ǉ�
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

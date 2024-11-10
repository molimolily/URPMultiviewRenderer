using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiviewRenderPass : ScriptableRenderPass
{
    Vector2Int currentResolution;
    RTHandle colorTarget;
    RTHandle depthTarget;

    Material blitMat;
    
    public MultiviewRenderPass()
    {
        blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/BlitToCameraRT"));
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        Vector2Int resolution = new Vector2Int(cameraTextureDescriptor.width, cameraTextureDescriptor.height);

        // �����_�[�^�[�Q�b�g��null�A�܂��͉𑜓x���ύX���ꂽ�ꍇ�Ƀ����_�[�^�[�Q�b�g���m��
        if(colorTarget != null || depthTarget != null ||  currentResolution != resolution)
        {
            // �����_�[�^�[�Q�b�g�̉��
            colorTarget?.Release();
            depthTarget?.Release();

            // �����_�[�^�[�Q�b�g�̊m��
            colorTarget = RTHandles.Alloc(cameraTextureDescriptor.width, cameraTextureDescriptor.height,
                1, DepthBits.None, GraphicsFormat.R8G8B8A8_SRGB, FilterMode.Bilinear,
                TextureWrapMode.Clamp, name: "_CameraColorTexture");

            depthTarget = RTHandles.Alloc(cameraTextureDescriptor.width, cameraTextureDescriptor.height,
                1, DepthBits.Depth32, GraphicsFormat.R32_SFloat, FilterMode.Point,
                TextureWrapMode.Clamp, name: "_CameraDepthTexture");

            currentResolution = resolution;
        }

        // �����_�[�^�[�Q�b�g�̐ݒ�
        ConfigureTarget(colorTarget, depthTarget);

        // �����_�[�^�[�Q�b�g�̃N���A
        ConfigureClear(ClearFlag.All, Color.clear);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("MultiviewRenderPass");

        // Rener: colorTarget, depthTarget�ɕ`��
        Render(context, ref renderingData);

        // �����_�[�^�[�Q�b�g�̉���
        ResetTarget();

        using (new ProfilingScope(cmd, new ProfilingSampler("Blit")))
        {
            // Blit
            if (blitMat != null)
            {
                Blitter.BlitCameraTexture(cmd, colorTarget, k_CameraTarget, blitMat, 0);
            }
        }

        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }


    void Render(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // �s�����I�u�W�F�N�g�̕`��
        SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

        // �X�J�C�{�b�N�X�̕`��
        if (renderingData.cameraData.camera.clearFlags == CameraClearFlags.Skybox)
        {
            context.DrawSkybox(renderingData.cameraData.camera);
        }

        // �����I�u�W�F�N�g�̕`��
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
    }

}

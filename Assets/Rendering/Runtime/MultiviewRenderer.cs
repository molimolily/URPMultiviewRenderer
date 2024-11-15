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

            // �}���`�r���[�����_�[�p�X
            multiviewRenderPass = new MultiviewRenderPass();

            int maxTextureArraySlices = SystemInfo.supports2DArrayTextures ? SystemInfo.maxTextureArraySlices : 0;
            Debug.Log("Max Texture2DArray Slices: " + maxTextureArraySlices);

            // �}�[�W�}�e���A���̐ݒ�
            if (mergeShader == null)
                mergeShader = Shader.Find("Merge/TilingRTArray");
            mergeMaterial = CoreUtils.CreateEngineMaterial(mergeShader);

            // �}�[�W�p�X
            mergeRTArrayPass = new MergeRTArrayPass(mergeMaterial);
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor camTexDesc = cameraData.cameraTargetDescriptor;
            Vector2Int resolution = new Vector2Int(camTexDesc.width, camTexDesc.height);

            // �J�������Ƃ̃y�C���[�h���擾
            if(!payloadCache.TryGetValue(cameraData.camera, out ICameraPayload payload))
            {
                payload = cameraData.camera.GetComponent<ICameraPayload>();
                payloadCache.Add(cameraData.camera, payload);
            }

            // �y�C���[�h��null�̏ꍇ�̓����_�����O���s��Ȃ�
            if (payload == null)
            {
                Debug.LogWarning("ICameraPayload is not attached to the camera. Rendering is not performed.");
                return;
            }

#if UNITY_EDITOR
            // �X���C�X���Ǝ��_���̃`�F�b�N
            if (payload.ViewCount.x * payload.ViewCount.y > SystemInfo.maxTextureArraySlices)
            {
                Debug.LogWarning("The number of slices exceeds the maximum number of slices supported by the device.");
                return;
            }
#endif

            // �����_�[�^�[�Q�b�g�̐���
            if (payload.ColorTarget == null || payload.DepthTarget == null)
            {
                Debug.Log("Generate Render Target");
                payload.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // ���_���ƃX���C�X�����قȂ�ꍇ�̓����_�[�^�[�Q�b�g���Ċm��
            if (payload.ColorTarget.rt.volumeDepth != payload.ViewCount.x * payload.ViewCount.y)
            {
                Debug.Log("Reallocate Render Target");
                payload.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // �X�N���[�����T�C�Y���̏���
            if (currentResolution.x != resolution.x || currentResolution.y != resolution.y)
            {
                Debug.Log("Screen Resize");
                currentResolution = resolution;
                payload.OnScreenResize(resolution.x, resolution.y);
            }

            // �����_�[�^�[�Q�b�g�̐ݒ�
            multiviewRenderPass.SetTarget(payload.ColorTarget, payload.DepthTarget);

            // ���_���̐ݒ�
            multiviewRenderPass.viewCount = payload.ViewCount;

            // �����_�[�e�N�X�`���̐ݒ�
            mergeRTArrayPass.SetInput(payload.ColorTarget);

            // pass�̒ǉ�
            EnqueuePass(multiviewRenderPass);
            EnqueuePass(mergeRTArrayPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(mergeMaterial);
        }
    }

}
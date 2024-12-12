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

            // �}���`�r���[�����_�[�p�X
            multiviewRenderPass = new MultiviewRenderPass();

            // �}�[�W�}�e���A���̐ݒ�
            if (mergeShader == null)
                mergeShader = Shader.Find("Merge/TilingRTArray");
            mergeMaterial = CoreUtils.CreateEngineMaterial(mergeShader);

            // �}�[�W�p�X
            mergeRTArrayPass = new MergeRTArrayPass(mergeMaterial);

            // ���C�e�B���O�̐ݒ�
            forwardLights = new ForwardLights();
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor camTexDesc = cameraData.cameraTargetDescriptor;
            Vector2Int resolution = new Vector2Int(camTexDesc.width, camTexDesc.height);

            int cameraID = cameraData.camera.GetHashCode();
            // �J�������Ƃ̃n���h�����擾
            if (!cameraHandlerCache.TryGetValue(cameraID, out IMultiviewCameraHandler handler))
            {
                handler = cameraData.camera.GetComponent<IMultiviewCameraHandler>();
                cameraHandlerCache.Add(cameraID, handler);
            }

            // �n���h����null�̏ꍇ�̓����_�����O���s��Ȃ�
            if (handler == null)
            {
                Debug.LogWarning("ICameraPayload is not attached to the camera. Rendering is not performed.");
                return;
            }

            // �����_�����O�̗L��
            if (!handler.ShouldRender)
            {
                return;
            }

#if UNITY_EDITOR
            // �X���C�X���Ǝ��_���̃`�F�b�N
            if (handler.ViewCount.x * handler.ViewCount.y > SystemInfo.maxTextureArraySlices)
            {
                Debug.LogWarning("The number of slices exceeds the maximum number of slices supported by the device.");
                return;
            }
#endif

            // �X�N���[�����T�C�Y���̏���
            // NOTE: �Œ�𑜓x�̂Ƃ��͏��������̂�, aspect��ŉ𑜓x���w�肵�Ă���ꍇ�Ƀ��T�C�Y����������������
            if (currentResolution.x != resolution.x || currentResolution.y != resolution.y)
            {
                currentResolution = resolution;
                handler.OnScreenResize(resolution.x, resolution.y);
            }

            // �����_�[�^�[�Q�b�g��null�`�F�b�N
            if (handler.ColorTarget == null || handler.DepthTarget == null)
            {
                handler.GenerateRenderTarget(resolution.x, resolution.y);
            }

            // �����_�[�^�[�Q�b�g�̐ݒ�
            multiviewRenderPass.SetTarget(handler.ColorTarget, handler.DepthTarget, handler.ScaleFactor);

            // ���_���̐ݒ�
            multiviewRenderPass.viewCount = handler.ViewCount;


            // Global�ϐ��̐ݒ�
            CommandBuffer cmd = CommandBufferPool.Get("Setup");
            cmd.SetGlobalInt("_ViewCountX", handler.ViewCount.x);
            cmd.SetGlobalInt("_ViewCountY", handler.ViewCount.y);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            // �r���[�f�[�^�̐ݒ�
            handler.SetViewData(context, ref renderingData);

            // �����_�[�e�N�X�`���̐ݒ�
            mergeRTArrayPass.SetInput(handler.ColorTarget, handler.ScaleFactor);

            // merge material�̃Z�b�g�A�b�v
            handler.SetupMergeMaterial(mergeMaterial);

            // pass�̒ǉ�
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
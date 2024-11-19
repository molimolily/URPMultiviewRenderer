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

        public static readonly GlobalKeyword multiview_Keyword = GlobalKeyword.Create("MULTIVIEW_PASS");

        static readonly int viewMatricesID = Shader.PropertyToID("_Multiview_ViewMatrices");
        static readonly int projectionMatricesID = Shader.PropertyToID("_Multiview_ProjectionMatrices");

        public MultiviewRenderer(Shader mergeShader, ScriptableRendererData data) : base(data)
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

            // ���C�e�B���O�̐ݒ�
            forwardLights = new ForwardLights();
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTextureDescriptor camTexDesc = cameraData.cameraTargetDescriptor;
            Vector2Int resolution = new Vector2Int(camTexDesc.width, camTexDesc.height);

            int cameraID = cameraData.camera.GetHashCode();
            // �J�������Ƃ̃y�C���[�h���擾
            if (!cameraPayloadCache.TryGetValue(cameraID, out ICameraPayload payload))
            {
                payload = cameraData.camera.GetComponent<ICameraPayload>();
                cameraPayloadCache.Add(cameraID, payload);
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

            // �r���[�f�[�^�̐ݒ�
            payload.SetViewData(context, ref renderingData);

            // �����_�[�e�N�X�`���̐ݒ�
            mergeRTArrayPass.SetInput(payload.ColorTarget);

            // pass�̒ǉ�
            EnqueuePass(multiviewRenderPass);
            EnqueuePass(mergeRTArrayPass);

            CommandBuffer cmd = CommandBufferPool.Get("Setup");
            cmd.EnableKeyword(multiview_Keyword);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FinishRendering(CommandBuffer cmd)
        {
            cmd.DisableKeyword(multiview_Keyword);
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
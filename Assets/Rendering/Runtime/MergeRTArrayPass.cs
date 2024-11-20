using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    public class MergeRTArrayPass : ScriptableRenderPass
    {
        Material mergeMaterial;
        RTHandle colorRTArray;

        public MergeRTArrayPass(Material mergeMaterial)
        {
            this.mergeMaterial = mergeMaterial;
            renderPassEvent = RenderPassEvent.AfterRendering;
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public void SetInput(RTHandle colorRTArray)
        {
            this.colorRTArray = colorRTArray;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("MergeRTArrayPass");

            // �C���X�^���X���̃��Z�b�g
            cmd.SetInstanceMultiplier(1);

            // �J�����̃r���[�|�[�g��ݒ�
            Rect camRect = renderingData.cameraData.camera.pixelRect;
            cmd.SetViewport(camRect);

            // TextureArray�̃^�C�����O
            if (mergeMaterial != null && colorRTArray != null)
            {
                // mergeMaterial.SetTexture("_ColorRTArray", colorRTArray.rt);
                CoreUtils.DrawFullScreen(cmd, mergeMaterial);
            }

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }
    }
}


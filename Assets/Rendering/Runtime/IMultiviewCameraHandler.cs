using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    /// <summary>
    /// �J�����̃y�C���[�h���`����C���^�[�t�F�[�X
    /// </summary>
    public interface IMultiviewCameraHandler
    {
        /// <summary>
        /// ���_��
        /// </summary>
        Vector2Int ViewCount { get; }

        // <summary>
        /// �����_�[�^�[�Q�b�g�n���h���̃v���p�e�B
        /// </summary>
        RTHandleProperties RenderTargetHandleProperties { get; }

        /// <summary>
        /// �J���[�����_�[�^�[�Q�b�g
        /// </summary>
        RTHandle ColorTarget { get; }

        /// <summary>
        /// �f�v�X�����_�[�^�[�Q�b�g
        /// </summary>
        RTHandle DepthTarget { get; }

        /// <summary>
        /// ��ʃ��T�C�Y���̏������s��
        /// Game View�̉𑜓x��aspect��Ŏw�肵�Ă���ꍇ�̓��T�C�Y������������
        /// </summary>
        /// <param name="width">��ʂ̕�</param>
        /// <param name="height">��ʂ̍���</param>
        void OnScreenResize(int width, int height);

        /// <summary>
        /// �����_�[�^�[�Q�b�g�̐����������s��
        /// �����_�[�^�[�Q�b�g�̏��������A�܂��͕`�揈���O�Ƀ����_�[�^�[�Q�b�g��null�̏ꍇ�ɌĂяo�����
        /// </summary>
        void GenerateRenderTarget(int width, int height);

        /// <summary>
        /// renderer��Setup()���ŌĂяo�����
        /// ���_���Ƃ̃f�[�^��ݒ肷��
        /// </summary>
        void SetViewData(ScriptableRenderContext context, ref RenderingData renderingData);

        /// <summary>
        /// merge material�̐ݒ�
        /// </summary>
        void SetupMergeMaterial(Material material);
    }
}

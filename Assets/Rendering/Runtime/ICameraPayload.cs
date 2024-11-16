using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MVR
{
    /// <summary>
    /// �J�����̃y�C���[�h���`����C���^�[�t�F�[�X
    /// </summary>
    public interface ICameraPayload
    {
        /// <summary>
        /// ���_��
        /// </summary>
        Vector2Int ViewCount { get; }

        /// <summary>
        /// �e���_�̃r���[�s��
        /// </summary>
        List<Matrix4x4> ViewMatrices { get; }

        /// <summary>
        /// �e���_�̃v���W�F�N�V�����s��
        /// </summary>
        List<Matrix4x4> ProjectionMatrices { get; }

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
        /// </summary>
        /// <param name="width">�V������ʂ̕�</param>
        /// <param name="height">�V������ʂ̍���</param>
        void OnScreenResize(int width, int height);

        /// <summary>
        /// �����_�[�^�[�Q�b�g�̐����������s��
        /// �����_�[�^�[�Q�b�g�̏��������A�܂��͕`�揈���O�Ƀ����_�[�^�[�Q�b�g��null�̏ꍇ�ɌĂяo�����
        /// </summary>
        void GenerateRenderTarget(int width, int height);
    }
}

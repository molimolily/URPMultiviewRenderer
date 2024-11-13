using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MVR
{
    public interface ICameraPayload
    {
        // ���_��
        Vector2Int ViewCount { get; }

        // �e���_�̃r���[�s��
        List<Matrix4x4> ViewMatrices { get; }

        // �e���_�̃v���W�F�N�V�����s��
        List<Matrix4x4> ProjectionMatrices { get; }

        // �����_�[�^�[�Q�b�g
        RTHandle ColorTarget { get; }
        RTHandle DepthTarget { get; }
    }
}

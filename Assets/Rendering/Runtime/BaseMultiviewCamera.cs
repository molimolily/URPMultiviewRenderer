using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MVR;

public abstract class BaseMultiviewCamera : MonoBehaviour
{
    /// <summary>
    /// �r���[�̏����𑜓x
    /// </summary>
    /// <param name="viewCount">���_��</param>
    /// <param name="width">�����_�[�^�[�Q�b�g�̕�</param>
    /// <param name="height">�����_�[�^�[�Q�b�g�̍���</param>
    /// <returns></returns>
    public abstract Vector2Int InitialViewResolution(Vector2Int viewCount, int width, int height);

    /// <summary>
    /// ���_�𑜓x�̌v�Z
    /// </summary>
    /// <param name="viewCount">���_��</param>
    /// <param name="width">�����_�[�^�[�Q�b�g�̕�</param>
    /// <param name="height">�����_�[�^�[�Q�b�g�̍���</param>
    /// <returns></returns>

    public abstract Vector2Int ComputeViewResolution(Vector2Int viewCount, int width, int height);
    public abstract void SetPerViewData(Vector2Int viewCount, int x, int y, out PerViewData perViewData);
    public abstract void SetupMergeMaterial(Material mergeMaterial);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MVR;

public abstract class BaseMultiviewCamera : MonoBehaviour
{
    /// <summary>
    /// ビューの初期解像度
    /// </summary>
    /// <param name="viewCount">視点数</param>
    /// <param name="width">レンダーターゲットの幅</param>
    /// <param name="height">レンダーターゲットの高さ</param>
    /// <returns></returns>
    public abstract Vector2Int InitialViewResolution(Vector2Int viewCount, int width, int height);

    /// <summary>
    /// 視点解像度の計算
    /// </summary>
    /// <param name="viewCount">視点数</param>
    /// <param name="width">レンダーターゲットの幅</param>
    /// <param name="height">レンダーターゲットの高さ</param>
    /// <returns></returns>

    public abstract Vector2Int ComputeViewResolution(Vector2Int viewCount, int width, int height);
    public abstract void SetPerViewData(Vector2Int viewCount, int x, int y, out PerViewData perViewData);
    public abstract void SetupMergeMaterial(Material mergeMaterial);
}

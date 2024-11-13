using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MVR
{
    public interface ICameraPayload
    {
        // 視点数
        Vector2Int ViewCount { get; }

        // 各視点のビュー行列
        List<Matrix4x4> ViewMatrices { get; }

        // 各視点のプロジェクション行列
        List<Matrix4x4> ProjectionMatrices { get; }

        // レンダーターゲット
        RTHandle ColorTarget { get; }
        RTHandle DepthTarget { get; }
    }
}

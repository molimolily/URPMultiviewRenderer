using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MVR
{
    /// <summary>
    /// カメラのペイロードを定義するインターフェース
    /// </summary>
    public interface ICameraPayload
    {
        /// <summary>
        /// 視点数
        /// </summary>
        Vector2Int ViewCount { get; }

        /// <summary>
        /// 各視点のビュー行列
        /// </summary>
        List<Matrix4x4> ViewMatrices { get; }

        /// <summary>
        /// 各視点のプロジェクション行列
        /// </summary>
        List<Matrix4x4> ProjectionMatrices { get; }

        /// <summary>
        /// カラーレンダーターゲット
        /// </summary>
        RTHandle ColorTarget { get; }

        /// <summary>
        /// デプスレンダーターゲット
        /// </summary>
        RTHandle DepthTarget { get; }

        /// <summary>
        /// 画面リサイズ時の処理を行う
        /// </summary>
        /// <param name="width">新しい画面の幅</param>
        /// <param name="height">新しい画面の高さ</param>
        void OnScreenResize(int width, int height);

        /// <summary>
        /// レンダーターゲットの生成処理を行う
        /// レンダーターゲットの初期化時、または描画処理前にレンダーターゲットがnullの場合に呼び出される
        /// </summary>
        void GenerateRenderTarget(int width, int height);
    }
}

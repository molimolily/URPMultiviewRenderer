using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace MVR
{
    [RequireComponent(typeof(Camera)), ExecuteAlways]
    public class MultiviewCameraPayload : MonoBehaviour, ICameraPayload, IDisposable
    {
        Camera cam;

        // NOTE: 変更をEditorに即時反映させるためにはSerializeしない
        [NonSerialized] Vector2Int _viewCount = new Vector2Int(1, 1);

        List<Matrix4x4> _viewMatrices = new List<Matrix4x4>();
        List<Matrix4x4> _projectionMatrices = new List<Matrix4x4>();

        RTHandle _colorTarget;
        RTHandle _depthTarget;

        public Vector2Int ViewCount => _viewCount;

        public int TotalViewCount => ViewCount.x * ViewCount.y;

        public List<Matrix4x4> ViewMatrices => _viewMatrices;
        public List<Matrix4x4> ProjectionMatrices => _projectionMatrices;
        public RTHandle ColorTarget => _colorTarget;
        public RTHandle DepthTarget => _depthTarget;

        void AllocateRenderTarget(int width, int height)
        {
            // レンダーターゲットの解放
            _colorTarget?.Release();
            _depthTarget?.Release();

            // レンダーターゲットの生成
            _colorTarget = RTHandles.Alloc(
                    width: width / ViewCount.x,
                    height: height / ViewCount.y,
                    slices: TotalViewCount,
                    depthBufferBits: DepthBits.None,
                    colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                    filterMode: FilterMode.Bilinear,
                    dimension: TextureDimension.Tex2DArray
            );

            _depthTarget = RTHandles.Alloc(
                width: width / ViewCount.x,
                height: height / ViewCount.y,
                slices: TotalViewCount,
                depthBufferBits: DepthBits.Depth32,
                colorFormat: GraphicsFormat.R32_SFloat,
                filterMode: FilterMode.Point,
                dimension: TextureDimension.Tex2DArray
                );
        }

        public void OnScreenResize(int width, int height)
        {
            AllocateRenderTarget(width, height);
        }

        public void GenerateRenderTarget(int width, int height)
        {
            AllocateRenderTarget(width, height);
        }

        void UpdateViewMatrices()
        {
            // ビュー行列の割当
            if (ViewMatrices.Count != TotalViewCount)
            {
                if (ViewMatrices.Capacity < TotalViewCount)
                {
                    ViewMatrices.Capacity = TotalViewCount;
                }
                ViewMatrices.Clear();
                for (int i = 0; i < TotalViewCount; i++)
                {
                    ViewMatrices.Add(Matrix4x4.identity);
                }
            }

            // ビュー行列の更新
            for (int y = 0; y < ViewCount.y; y++)
            {
                for (int x = 0; x < ViewCount.x; x++)
                {
                    int index = x + y * ViewCount.x;
                    var viewMatrix = cam.worldToCameraMatrix;
                    ViewMatrices[index] = viewMatrix;
                }
            }
        }

        void UpdateProjectionMatrices()
        {
            // プロジェクション行列の割当
            if (ProjectionMatrices.Count != TotalViewCount)
            {
                if (ProjectionMatrices.Capacity < TotalViewCount)
                {
                    ProjectionMatrices.Capacity = TotalViewCount;
                }
                ProjectionMatrices.Clear();
                for (int i = 0; i < TotalViewCount; i++)
                {
                    ProjectionMatrices.Add(Matrix4x4.identity);
                }
            }
            // プロジェクション行列の更新
            for (int y = 0; y < ViewCount.y; y++)
            {
                for (int x = 0; x < ViewCount.x; x++)
                {
                    int index = x + y * ViewCount.x;
                    var projMatrix = cam.projectionMatrix;
                    ProjectionMatrices[index] = projMatrix;
                }
            }
        }

        void UpdateMatrices()
        {
            UpdateViewMatrices();
            UpdateProjectionMatrices();
        }

        void Update()
        {
            // カメラの取得
            if (cam == null)
            {
                cam = GetComponent<Camera>();
            }
        }

        void LateUpdate()
        {
            // 行列の更新
            UpdateMatrices();
        }

        public void Dispose()
        {
            Debug.Log("Multiview Camera Payload Dispose");
            _colorTarget?.Release();
            _depthTarget?.Release();
        }
    }
}


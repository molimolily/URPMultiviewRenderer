using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MVR
{
    [RequireComponent(typeof(Camera)), ExecuteAlways]
    public class MultiviewCameraPayload : MonoBehaviour, ICameraPayload, IDisposable
    {
        Camera cam;
        Vector2Int currentResolution;

        Vector2Int _viewCount = new Vector2Int(1, 1);

        List<Matrix4x4> _viewMatrices = new List<Matrix4x4>();
        List<Matrix4x4> _projectionMatrices = new List<Matrix4x4>();

        RTHandle _colorTarget;
        RTHandle _depthTarget;

        public Vector2Int ViewCount
        {
            get => _viewCount;
            set
            {
                if (_viewCount != value)
                {
                    _viewCount = value;
                    AllocateRenderTarget();
                }
            }
        }

        public List<Matrix4x4> ViewMatrices => _viewMatrices;
        public List<Matrix4x4> ProjectionMatrices => _projectionMatrices;
        public RTHandle ColorTarget => _colorTarget;
        public RTHandle DepthTarget => _depthTarget;

        void AllocateRenderTarget()
        {

        }

        void UpdateMatrices()
        {

        }

        bool HasResolutionChanged()
        {
            if(currentResolution.x != cam.pixelWidth || currentResolution.y != cam.pixelHeight)
            {
                currentResolution = new Vector2Int(cam.pixelWidth, cam.pixelHeight);
                return true;
            }
            return false;
        }

        void Update()
        {
            // カメラの取得
            if (cam == null)
            {
                cam = GetComponent<Camera>();
                currentResolution = new Vector2Int(cam.pixelWidth, cam.pixelHeight);
            }

            // 解像度が変更された場合はレンダーターゲットを再確保
            if (HasResolutionChanged())
            {
                AllocateRenderTarget();
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


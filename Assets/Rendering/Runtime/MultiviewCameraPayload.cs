using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;

namespace MVR
{
    [RequireComponent(typeof(Camera)), ExecuteAlways]
    public class MultiviewCameraPayload : MonoBehaviour, ICameraPayload
    {
        Camera cam;

        // NOTE: 変更をEditorに即時反映させるためにはSerializeしない
        [NonSerialized] Vector2Int _viewCount = new Vector2Int(20, 20);

        List<PerViewData> perViewData = new List<PerViewData>();
        GraphicsBuffer perViewDataBuffer;
        static readonly int perViewDataID = Shader.PropertyToID("_PerViewData");

        RTHandle _colorTarget;
        RTHandle _depthTarget;

        public Vector2Int ViewCount => _viewCount;

        public int TotalViewCount => ViewCount.x * ViewCount.y;

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

        public void SetViewData(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // bufferの生成
            if (perViewDataBuffer == null || perViewDataBuffer.count != TotalViewCount)
            {
                perViewDataBuffer?.Release();
                perViewDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, TotalViewCount, PerViewData.Size);
            }

            // bufferの更新
            perViewDataBuffer.SetData(perViewData);

            // bufferの送信
            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.SetGlobalBuffer(perViewDataID, perViewDataBuffer);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void SetupMergeMaterial(Material material)
        {
            if(material == null) return;
            if(_colorTarget == null) return;

            material.SetTexture("_ColorRTArray", _colorTarget);
        }

        void UpdatePerViewData()
        {
            // PerViewDataの割当
            if (perViewData.Count != TotalViewCount)
            {
                if (perViewData.Capacity < TotalViewCount)
                {
                    perViewData.Capacity = TotalViewCount;
                }
                perViewData.Clear();
                for (int i = 0; i < TotalViewCount; i++)
                {
                    perViewData.Add(new PerViewData
                    {
                        viewMatrix = float4x4.identity,
                        projectionMatrix = float4x4.identity
                    });
                }
            }

            // PerViewDataの更新
            for (int y = 0; y < ViewCount.y; y++)
            {
                for (int x = 0; x < ViewCount.x; x++)
                {
                    int index = x + y * ViewCount.x;
                    Vector3 pos = cam.transform.position;
                    cam.transform.position = new Vector3(pos.x + (x - (ViewCount.x - 1) / 2.0f) * 0.1f, pos.y - (y - (ViewCount.y - 1) / 2.0f) * 0.1f, pos.z);
                    float4x4 viewMatrix = cam.worldToCameraMatrix;
                    cam.transform.position = pos;
                    perViewData[index] = new PerViewData
                    {
                        viewMatrix = viewMatrix,
                        projectionMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true)
                    };
                }
            }
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
            // PerViewDataの更新
            UpdatePerViewData();
        }

        void OnDisable()
        {
            ReleaseResource();
        }

        void OnDestroy()
        {
            ReleaseResource();
        }

        public void ReleaseResource()
        {
            _colorTarget?.Release();
            _colorTarget = null;
            _depthTarget?.Release();
            _depthTarget = null;
            perViewDataBuffer?.Release();
            perViewDataBuffer = null;
        }
    }
}


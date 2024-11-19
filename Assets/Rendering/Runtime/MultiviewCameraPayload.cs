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
    public class MultiviewCameraPayload : MonoBehaviour, ICameraPayload, IDisposable
    {
        Camera cam;

        // NOTE: �ύX��Editor�ɑ������f�����邽�߂ɂ�Serialize���Ȃ�
        [NonSerialized] Vector2Int _viewCount = new Vector2Int(2, 2);

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
            // �����_�[�^�[�Q�b�g�̉��
            _colorTarget?.Release();
            _depthTarget?.Release();

            // �����_�[�^�[�Q�b�g�̐���
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
            // buffer�̐���
            if (perViewDataBuffer == null)
            {
                perViewDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, TotalViewCount, PerViewData.Size);
            }

            // buffer�̍X�V
            perViewDataBuffer.SetData(perViewData);

            // buffer�̑��M
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Set PerViewData")))
            {
                cmd.SetGlobalBuffer(perViewDataID, perViewDataBuffer);
                context.ExecuteCommandBuffer(cmd);
            }
            CommandBufferPool.Release(cmd);
        }

        void UpdatePerViewData()
        {
            // PerViewData�̊���
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

            // PerViewData�̍X�V
            for (int y = 0; y < ViewCount.y; y++)
            {
                for (int x = 0; x < ViewCount.x; x++)
                {
                    int index = x + y * ViewCount.x;
                    perViewData[index] = new PerViewData
                    {
                        viewMatrix = cam.worldToCameraMatrix,
                        projectionMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true)
                    };
                }
            }
        }


        void Update()
        {
            // �J�����̎擾
            if (cam == null)
            {
                cam = GetComponent<Camera>();
            }
        }

        void LateUpdate()
        {
            // PerViewData�̍X�V
            UpdatePerViewData();
        }

        public void Dispose()
        {
            Debug.Log("Multiview Camera Payload Dispose");
            _colorTarget?.Release();
            _depthTarget?.Release();
            perViewDataBuffer?.Release();
        }
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    [RequireComponent(typeof(Camera)), ExecuteAlways]
    public class MultiviewCameraHandler : MonoBehaviour, IMultiviewCameraHandler
    {
        Camera cam;

        // NOTE: 変更をEditorに即時反映させるためにはSerializeしない
        [NonSerialized] Vector2Int _viewCount = new Vector2Int(2, 2);

        List<PerViewData> perViewData = new List<PerViewData>();
        GraphicsBuffer perViewDataBuffer;
        static readonly int perViewDataID = Shader.PropertyToID("_PerViewData");

        RTHandleSystem rtHandleSytem;

        public Vector2Int ViewCount
        {
            get => _viewCount;
            set
            {
                if(ViewCount != value)
                {
                    _viewCount = value;
                    AllocateRenderTarget(ColorTarget.rt.width, ColorTarget.rt.height);
                }
            }
        }

        public int TotalViewCount => ViewCount.x * ViewCount.y;

        public RTHandleProperties RenderTargetHandleProperties => rtHandleSytem.rtHandleProperties;
        public RTHandle ColorTarget { get; private set; }
        public RTHandle DepthTarget { get; private set; }

        void AllocateRenderTarget(int width, int height)
        {
            // 各視点の解像度
            int viewWidth = Mathf.CeilToInt(width / ViewCount.x);
            int viewHeight = Mathf.CeilToInt(height / ViewCount.y);

            // ReferenceSizeの設定
            rtHandleSytem.SetReferenceSize(viewWidth, viewHeight);

            RTHandleProperties rtHandleProperties = rtHandleSytem.rtHandleProperties;

            if (ColorTarget == null || DepthTarget == null || 
                ColorTarget.rt.volumeDepth != TotalViewCount || DepthTarget.rt.volumeDepth != TotalViewCount)
            {
                // レンダーターゲットの解放
                ColorTarget?.Release();
                DepthTarget?.Release();

                // レンダーターゲットの生成
                ColorTarget = rtHandleSytem.Alloc(
                        scaleFactor: Vector2.one,
                        slices: TotalViewCount,
                        depthBufferBits: DepthBits.None,
                        colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                        filterMode: FilterMode.Bilinear,
                        wrapMode: TextureWrapMode.Clamp,
                        dimension: TextureDimension.Tex2DArray,
                        name: "ColorTargetArray"
                );

                DepthTarget = rtHandleSytem.Alloc(
                    scaleFactor: Vector2.one,
                    slices: TotalViewCount,
                    depthBufferBits: DepthBits.Depth32,
                    colorFormat: GraphicsFormat.R32_SFloat,
                    filterMode: FilterMode.Point,
                    wrapMode: TextureWrapMode.Clamp,
                    dimension: TextureDimension.Tex2DArray,
                    name: "DepthTargetArray"
                    );
            }

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
                        viewMatrix = Matrix4x4.identity,
                        projectionMatrix = Matrix4x4.identity
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
                    Matrix4x4 viewMatrix = cam.worldToCameraMatrix;
                    cam.transform.position = pos;
                    perViewData[index] = new PerViewData
                    {
                        viewMatrix = viewMatrix,
                        projectionMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true)
                    };
                }
            }
        }

        void Init()
        {
            // カメラの取得
            cam = GetComponent<Camera>();

            // RTHandleSystemの初期化
            rtHandleSytem = new RTHandleSystem();
            int width = cam.pixelWidth / ViewCount.x;
            int height = cam.pixelHeight / ViewCount.y;
            rtHandleSytem.Initialize(width, height);
        }

        void OnEnable()
        {
            Init();
        }

        void Update()
        {
#if UNITY_EDITOR
            if(cam == null || rtHandleSytem == null)
            {
                Init();
            }
#endif
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
            ColorTarget?.Release();
            ColorTarget = null;
            DepthTarget?.Release();
            DepthTarget = null;
            perViewDataBuffer?.Release();
            perViewDataBuffer = null;
        }
    }
}


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

        List<SingleViewCamera> cameras = new List<SingleViewCamera>();
        List<PerViewData> perViewData = new List<PerViewData>();
        GraphicsBuffer perViewDataBuffer;
        static readonly int perViewDataID = Shader.PropertyToID("_PerViewData");

        RTHandleSystem rtHandleSytem;

        public BaseMultiviewCamera multiviewCamera;

        public bool ShouldRender { get; set; } = false;

        [SerializeField, HideInInspector] Vector2Int _viewCount = Vector2Int.one;
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
            bool hasRenderTargets = ColorTarget != null && DepthTarget != null;
            bool isViewCountChanged = hasRenderTargets ? ColorTarget.rt.volumeDepth != TotalViewCount || DepthTarget.rt.volumeDepth != TotalViewCount : false;
            if (isViewCountChanged)
            {
                InitializeRTHandleSystem();
                Debug.Log("ViewCount Changed");
            }

            // �e���_�̉𑜓x
            Vector2Int viewResolution = multiviewCamera.ComputeViewResolution(ViewCount, width, height);

            // ReferenceSize�̐ݒ�
            // rtHandleSytem.SetReferenceSize(viewResolution.x, viewResolution.y, isViewCountChanged);
            if(isViewCountChanged)
            {
                rtHandleSytem.ResetReferenceSize(viewResolution.x, viewResolution.y);
            }
            else
            {
                rtHandleSytem.SetReferenceSize(viewResolution.x, viewResolution.y);
            }

            // RTHandleProperties�̎擾
            RTHandleProperties rtHandleProperties = rtHandleSytem.rtHandleProperties;

            if (!hasRenderTargets || isViewCountChanged)
            {
                // �����_�[�^�[�Q�b�g�̉��
                ColorTarget?.Release();
                DepthTarget?.Release();

                // �����_�[�^�[�Q�b�g�̐���
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

        /// <summary>
        /// �e���_�̃r���[�f�[�^��GPU�ɑ��M����
        /// </summary>
        /// <param name="context"></param>
        /// <param name="renderingData"></param>
        public virtual void SetViewData(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // buffer�̐���
            if (perViewDataBuffer == null || perViewDataBuffer.count != TotalViewCount)
            {
                perViewDataBuffer?.Release();
                perViewDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, TotalViewCount, PerViewData.Size);
            }

            // buffer�̍X�V
            perViewDataBuffer.SetData(perViewData);

            // buffer�̑��M
            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.SetGlobalBuffer(perViewDataID, perViewDataBuffer);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <summary>
        /// MergeMaterial�̐ݒ�
        /// </summary>
        public virtual void SetupMergeMaterial(Material material)
        {
            
        }

        /// <summary>
        /// �e���_�̃r���[�f�[�^���X�V����
        /// </summary>
        protected virtual void UpdatePerViewData()
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
                        viewMatrix = Matrix4x4.identity,
                        projectionMatrix = Matrix4x4.identity
                    });
                }
            }

            // PerViewData�̍X�V
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

        void InitializeRTHandleSystem()
        {
            rtHandleSytem?.Dispose();
            rtHandleSytem = new RTHandleSystem();

            Vector2Int viewResolution = multiviewCamera.InitialViewResolution(ViewCount, cam.pixelWidth, cam.pixelHeight);
            Debug.Log(viewResolution);
            rtHandleSytem.Initialize(viewResolution.x, viewResolution.y);
        }

            void Init()
        {
            if(multiviewCamera != null || ViewCount.x < 0 || ViewCount.y < 0)
                ShouldRender = true;
            else
                ShouldRender = false;

            // �J�����̎擾
            cam = GetComponent<Camera>();

            // RTHandleSystem�̏�����
            InitializeRTHandleSystem();
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
            // PerViewData�̍X�V
            UpdatePerViewData();
        }

        public void ReleaseResources()
        {
            ColorTarget?.Release();
            ColorTarget = null;
            DepthTarget?.Release();
            DepthTarget = null;
            perViewDataBuffer?.Release();
            perViewDataBuffer = null;
        }

        protected virtual void OnDisable() => ReleaseResources();
        protected virtual void OnDestroy() => ReleaseResources();
    }
}


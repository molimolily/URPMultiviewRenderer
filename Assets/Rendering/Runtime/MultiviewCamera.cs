using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MVR;
public class MultiviewCamera : BaseMultiviewCamera
{
    [SerializeField] float fov = 60.0f;
    [SerializeField] float near = 0.3f;
    [SerializeField] float far = 1000.0f;
    [SerializeField] float pitch = 0.5f;
    private Vector2Int viewResolution;
    public override Vector2Int InitialViewResolution(Vector2Int viewCount, int width, int height)
    {
        int viewWidth = Mathf.CeilToInt(width / viewCount.x);
        int viewHeight = Mathf.CeilToInt(height / viewCount.y);
        return new Vector2Int(viewWidth, viewHeight);
    }

    public override Vector2Int ComputeViewResolution(Vector2Int viewCount, int width, int height)
    {
        int viewWidth = Mathf.CeilToInt(width / viewCount.x);
        int viewHeight = Mathf.CeilToInt(height / viewCount.y);
        viewResolution = new Vector2Int(viewWidth, viewHeight);
        return new Vector2Int(viewWidth, viewHeight);
    }

    public override void SetPerViewData(Vector2Int viewCount, int x, int y, out PerViewData perViewData)
    {
        float aspect;
        if(viewResolution.x > 0 || viewResolution.y > 0)
            aspect = (float)viewResolution.x / viewResolution.y;
        else
            aspect = Screen.width / Screen.height;


        Vector3 pos = new Vector3(x - (viewCount.x - 1) / 2.0f, -(y - (viewCount.y - 1) / 2.0f), 0) * pitch;
        pos = transform.TransformPoint(pos);
        perViewData.viewMatrix = MatrixUtil.CreateViewMatrix(pos, transform.right.normalized, transform.up.normalized, -transform.forward);
        perViewData.projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(fov, aspect, near, far), true);
    }

    public override void SetupMergeMaterial(Material mergeMaterial)
    {
    }
}

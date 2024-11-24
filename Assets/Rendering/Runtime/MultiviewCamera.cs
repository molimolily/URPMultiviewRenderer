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


        Vector3 pos = transform.position + new Vector3(x - (viewCount.x - 1) / 2.0f, -(y - (viewCount.y - 1) / 2.0f), 0) * pitch;
        perViewData.viewMatrix = CreateViewMatrix(pos);
        perViewData.projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(fov, aspect, near, far), true);
    }

    Matrix4x4 CreateViewMatrix(Vector3 position)
    {
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        Vector3 forward = -transform.forward; // ÉJÉÅÉâÇÃëOï˚ÇÕïâÇÃï˚å¸

        Matrix4x4 viewMatrix = new Matrix4x4();

        viewMatrix.m00 = right.x;
        viewMatrix.m01 = right.y;
        viewMatrix.m02 = right.z;
        viewMatrix.m03 = -Vector3.Dot(right, position);

        viewMatrix.m10 = up.x;
        viewMatrix.m11 = up.y;
        viewMatrix.m12 = up.z;
        viewMatrix.m13 = -Vector3.Dot(up, position);

        viewMatrix.m20 = forward.x;
        viewMatrix.m21 = forward.y;
        viewMatrix.m22 = forward.z;
        viewMatrix.m23 = -Vector3.Dot(forward, position);

        viewMatrix.m30 = 0.0f;
        viewMatrix.m31 = 0.0f;
        viewMatrix.m32 = 0.0f;
        viewMatrix.m33 = 1.0f;
        return viewMatrix;
    }
}

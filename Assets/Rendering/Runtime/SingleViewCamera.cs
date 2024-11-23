using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleViewCamera : MonoBehaviour
{
    Matrix4x4 _viewMatrix = Matrix4x4.identity;
    public Matrix4x4 ViewMatrix 
    {
        get
        {
            UpdateViewMatrix();
            return _viewMatrix;
        }
    }

    Matrix4x4 _projectionMatrix = Matrix4x4.identity;
    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            UpdateProjectionMatrix();
            return _projectionMatrix;
        }
    }

    public FrustumPlanes frustum;

    /// <summary>
    /// Near cliping plane
    /// access to FrustumPlane.zNear
    /// </summary>
    public float Near
    {
        get => frustum.zNear;
        set
        {
            frustum.zNear = value;
            FoV = _fov;
        }
    }

    /// <summary>
    /// Far cliping plane
    /// access to FrustumPlane.zFar
    /// </summary>
    public float Far
    {
        get => frustum.zFar;
        set
        {
            frustum.zFar = value;
        }
    }

    private float _fov;
    /// <summary>
    /// Field of view [deg]
    /// FrustumPlaneの値を直接設定した場合、この値は適切な視野角を表さない
    /// </summary>
    public float FoV
    {
        
        get => _fov;
        set
        {
            frustum.top = Mathf.Tan(value * Mathf.Deg2Rad / 2) * frustum.zNear;
            frustum.bottom = -frustum.top;
            frustum.right = frustum.top * _aspect;
            frustum.left = -frustum.right;
            _fov = value;
        }
    }

    private float _aspect;
    /// <summary>
    /// アスペクト比
    /// FrustumPlaneの値を直接設定した場合、この値は適切なアスペクト比を表さない
    /// </summary>
    public float Aspect
    {
        get => _aspect;
        set
        {
            frustum.right = frustum.top * value;
            frustum.left = -frustum.right;
            _aspect = value;
        }
    }

    protected FrustumPlanes previousFrustumPlanes;
    protected Vector3 previousPosition = Vector3.zero;
    protected Quaternion previousRotation = Quaternion.identity;

    bool HasTransformChanged()
    {
        bool dirty = previousPosition != transform.position || previousRotation != transform.rotation;
        if(dirty)
        {
            previousPosition = transform.position;
            previousRotation = transform.rotation;
        }
        return dirty;
    }

    bool HasProjectionChanged()
    {
        var currentFrustum = frustum;
        bool dirty = previousFrustumPlanes.left != currentFrustum.left ||
            previousFrustumPlanes.right != currentFrustum.right ||
            previousFrustumPlanes.bottom != currentFrustum.bottom ||
            previousFrustumPlanes.top != currentFrustum.top ||
            previousFrustumPlanes.zNear != currentFrustum.zNear ||
            previousFrustumPlanes.zFar != currentFrustum.zFar;

        if (dirty)
        {
            previousFrustumPlanes = currentFrustum;
        }
        return dirty;
    }

    protected virtual void  UpdateViewMatrix()
    {
        if (HasTransformChanged())
            _viewMatrix = transform.worldToLocalMatrix;
    }

    protected virtual void UpdateProjectionMatrix()
    {
        if (HasProjectionChanged())
            _projectionMatrix = Matrix4x4.Frustum(frustum);
    }

    #region EditorOnly
#if UNITY_EDITOR
    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        DrawGizmoFrustum();
    }

    Vector3[] fc = new Vector3[8];
    Vector3[] fl = new Vector3[24];
    void DrawGizmoFrustum()
    {
        var f = frustum;
        // Near plane
        fc[0] = new Vector3(f.left, f.bottom, f.zNear);
        fc[1] = new Vector3(f.right, f.bottom, f.zNear);
        fc[2] = new Vector3(f.right, f.top, f.zNear);
        fc[3] = new Vector3(f.left, f.top, f.zNear);

        // Far plane
        fc[4] = new Vector3(f.left * f.zFar / f.zNear, f.bottom * f.zFar / f.zNear, f.zFar);
        fc[5] = new Vector3(f.right * f.zFar / f.zNear, f.bottom * f.zFar / f.zNear, f.zFar);
        fc[6] = new Vector3(f.right * f.zFar / f.zNear, f.top * f.zFar / f.zNear, f.zFar);
        fc[7] = new Vector3(f.left * f.zFar / f.zNear, f.top * f.zFar / f.zNear, f.zFar);

        // ワールド座標に変換
        transform.TransformPoints(fc);

        // Near plane
        fl[0] = fc[0]; fl[1] = fc[1];
        fl[2] = fc[1]; fl[3] = fc[2];
        fl[4] = fc[2]; fl[5] = fc[3];
        fl[6] = fc[3]; fl[7] = fc[0];

        // Far plane
        fl[8] = fc[4]; fl[9] = fc[5];
        fl[10] = fc[5]; fl[11] = fc[6];
        fl[12] = fc[6]; fl[13] = fc[7];
        fl[14] = fc[7]; fl[15] = fc[4];

        // Sides
        fl[16] = fc[0]; fl[17] = fc[4];
        fl[18] = fc[1]; fl[19] = fc[5];
        fl[20] = fc[2]; fl[21] = fc[6];
        fl[22] = fc[3]; fl[23] = fc[7];

        Gizmos.DrawLineList(fl);
    }
#endif
    #endregion
}

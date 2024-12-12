using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MVR;

public class CIICameraArray : BaseMultiviewCamera
{
    [SerializeField] Vector2 screenPhysicalSize = new Vector2(600.0f, 340.0f); // 0.1m
    [SerializeField] Vector2 lensSize = new Vector2(4.8f, 4.8f); // 4.8mm
    [SerializeField] float gap = 14.0f; // 14mm

    [SerializeField] float near = 0.3f;
    [SerializeField] float far = 1000.0f;
    [SerializeField] float scale = 0.1f;
    [SerializeField] Vector3 eyePosition = new Vector3(0.0f, 0.0f, 700.0f);

    [SerializeField] Vector2 screenOffset = new Vector2(0.0f, 0.0f);
    Vector4 offset = Vector4.zero;

    Vector2Int elementResolution;
    public override Vector2Int InitialViewResolution(Vector2Int viewCount, int width, int height)
    {
        elementResolution = ComputeElementResolution();
        return elementResolution;
    }

    public override Vector2Int ComputeViewResolution(Vector2Int viewCount, int width, int height)
    {
        elementResolution = ComputeElementResolution();
        return elementResolution;
    }

    public override void SetPerViewData(Vector2Int viewCount, int x, int y, out PerViewData perViewData)
    {
        Vector3 camPos = new Vector3(x - (viewCount.x - 1) / 2.0f, (y - (viewCount.y - 1) / 2.0f), 0) * lensSize * scale;
        camPos = transform.TransformPoint(camPos);
        perViewData.viewMatrix = MatrixUtil.CreateViewMatrix(camPos, transform.right.normalized, transform.up.normalized, -transform.forward);
        // perViewData.projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(10.0f, 1.0f, near, far), true);
        float left = ((x - viewCount.x / 2.0f + 1) / eyePosition.z + 0.5f / gap) * near * lensSize.x - near * eyePosition.x / eyePosition.z;
        float right = ((x - viewCount.x / 2.0f) / eyePosition.z - 0.5f / gap) * near * lensSize.x - near * eyePosition.x / eyePosition.z;
        float bottom = ((y - viewCount.y / 2.0f) / eyePosition.z - 0.5f / gap) * near * lensSize.y - near * eyePosition.y / eyePosition.z;
        float top = ((y - viewCount.y / 2.0f + 1) / eyePosition.z + 0.5f / gap) * near * lensSize.y - near * eyePosition.y / eyePosition.z;
        perViewData.projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Frustum(right, left, bottom, top, near, far), true);
    }

    public override void SetupMergeMaterial(Material mergeMaterial)
    {
        mergeMaterial.SetInt("_ElementWidth", elementResolution.x);
        mergeMaterial.SetInt("_ElementHeight", elementResolution.y);

        offset.x = screenOffset.x / screenPhysicalSize.x;
        offset.y = screenOffset.y / screenPhysicalSize.y;
        if (eyePosition.z > 0)
        {
            float eyeOffsetX = eyePosition.x * gap / eyePosition.z / screenPhysicalSize.x;
            float eyeOffsetY = eyePosition.y * gap / eyePosition.z / screenPhysicalSize.y;
            offset.z = eyeOffsetX;
            offset.w = eyeOffsetY;
        }
        mergeMaterial.SetVector("_Offset", offset);
    }

    Vector2Int ComputeElementResolution()
    {
        float width = Screen.width * lensSize.x / screenPhysicalSize.x;
        float height = Screen.height * lensSize.y / screenPhysicalSize.y;
        
        // Ž‹“_‹——£‚É‰ž‚¶‚Ä—v‘f‰æ‘œ‚ðŠg‘å
        if(eyePosition.z > 0)
        {
            width *= (1 + gap / eyePosition.z);
            height *= (1 + gap / eyePosition.z);
        }

        return new Vector2Int(Mathf.CeilToInt(width), Mathf.CeilToInt(height));
    }
}

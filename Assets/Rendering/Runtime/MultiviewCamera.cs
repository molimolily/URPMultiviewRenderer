using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MVR;
public class MultiviewCamera : BaseMultiviewCamera
{
    public override Vector2Int InitialViewResolution(Vector2Int viewCount, int width, int height)
    {
        return new Vector2Int(width / viewCount.x, height / viewCount.y);
    }

    public override Vector2Int ComputeViewResolution(Vector2Int viewCount, int width, int height)
    {
        return new Vector2Int(width / viewCount.x, height / viewCount.y);
    }

    public override void SetPerViewData(Vector2Int viewCount, int x, int y, out PerViewData perViewData)
    {
        perViewData.projectionMatrix = Matrix4x4.Perspective(60.0f, 16.0f / 9.0f, 0.3f, 1000.0f);
        perViewData.viewMatrix = Matrix4x4.identity;
    }
}

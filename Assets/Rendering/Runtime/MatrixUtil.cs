using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVR
{
    public static class MatrixUtil
    {
        public static Matrix4x4 CreateViewMatrix(Vector3 pos, Vector3 right, Vector3 up, Vector3 forward)
        {
            Matrix4x4 viewMatrix = new Matrix4x4();

            viewMatrix.m00 = right.x;
            viewMatrix.m01 = right.y;
            viewMatrix.m02 = right.z;
            viewMatrix.m03 = -Vector3.Dot(right, pos);

            viewMatrix.m10 = up.x;
            viewMatrix.m11 = up.y;
            viewMatrix.m12 = up.z;
            viewMatrix.m13 = -Vector3.Dot(up, pos);

            viewMatrix.m20 = forward.x;
            viewMatrix.m21 = forward.y;
            viewMatrix.m22 = forward.z;
            viewMatrix.m23 = -Vector3.Dot(forward, pos);

            viewMatrix.m30 = 0;
            viewMatrix.m31 = 0;
            viewMatrix.m32 = 0;
            viewMatrix.m33 = 1;

            return viewMatrix;
        }

        public static Matrix4x4 CreateViewMatrix(Transform transform)
        {
            Vector3 pos = transform.position;
            Vector3 right = transform.right.normalized;
            Vector3 up = transform.up.normalized;
            Vector3 forward = -transform.forward;

            return CreateViewMatrix(pos, right, up, forward);
        }
    }
}

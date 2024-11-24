using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ViewMatrixTest : MonoBehaviour
{
    Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        CheckMatrix();
    }

    Matrix4x4 CreateViewMatrix()
    {
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        Vector3 forward = -transform.forward; // �J�����̑O���͕��̕���
        Vector3 position = transform.position;

        // �r���[�s���������
        Matrix4x4 viewMatrix = new Matrix4x4();

        // �s��̊e�v�f��ݒ�
        viewMatrix.m00 = right.x;
        viewMatrix.m01 = right.y;
        viewMatrix.m02 = right.z;
        viewMatrix.m03 = -Vector3.Dot(right, position);

        viewMatrix.m10 = up.x;
        viewMatrix.m11 = up.y;
        viewMatrix.m12 = up.z;
        viewMatrix.m13 = -Vector3.Dot(up, position);

        viewMatrix.m20 = forward.x; // �O�����x�N�g���Ƀ}�C�i�X��t����
        viewMatrix.m21 = forward.y;
        viewMatrix.m22 = forward.z;
        viewMatrix.m23 = -Vector3.Dot(forward, position); // �}�C�i�X��2��|����̂Ńv���X

        viewMatrix.m30 = 0.0f;
        viewMatrix.m31 = 0.0f;
        viewMatrix.m32 = 0.0f;
        viewMatrix.m33 = 1.0f;
        return viewMatrix;
    }

    void CheckMatrix()
    {
        Matrix4x4 customMat = CreateViewMatrix();
        Matrix4x4 camMat = cam.worldToCameraMatrix;

        /*Debug.Log($"Chack: {customMat == camMat}\n" +
            $"customMat:\n" +
            $"{customMat.ToString()}\n" +
            $"camMat:\n" +
            $"{camMat.ToString()}");*/
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputCullingMatrix : MonoBehaviour
{
    [SerializeField] Camera cam;
    Matrix4x4 cullingMatrix;
    Matrix4x4 vpMatrix;
    // Start is called before the first frame update
    void Start()
    {
        cullingMatrix = cam.cullingMatrix;
        vpMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;
        Debug.Log("Culling Matrix: " + cullingMatrix);
        Debug.Log("Projection Matrix: " + vpMatrix);
        Debug.Log("Compare: " + (cullingMatrix == vpMatrix));
    }

    // Update is called once per frame
    void Update()
    {
        cullingMatrix = cam.cullingMatrix;
        vpMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;
        Debug.Log("Compare: " + (cullingMatrix == vpMatrix));
    }
}

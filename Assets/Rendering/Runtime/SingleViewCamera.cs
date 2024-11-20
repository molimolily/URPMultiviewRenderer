using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleViewCamera : MonoBehaviour
{
    public float fieldOfView = 60.0f;
    public float nearClipPlane = 0.3f;
    public float farClipPlane = 1000.0f;

    public Matrix4x4 ViewMatrix { get; private set; }
    public Matrix4x4 ProjectionMatrix { get; private set; }
    public Matrix4x4 ViewProjectionMatrix { get; private set; }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void UpdateViewMatrix()
    {

    }
}

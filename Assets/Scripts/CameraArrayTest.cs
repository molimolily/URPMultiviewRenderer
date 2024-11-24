using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraArrayTest : MonoBehaviour
{
    [SerializeField] Vector2Int viewCount = new Vector2Int(2, 2);
    [SerializeField] SingleViewCamera singleViewCameraPrefab;
    [SerializeField] BaseMultiviewCamera multiviewCamera;
    List<SingleViewCamera> camraArray = new List<SingleViewCamera>();

    void SetupCameraArray()
    {
        foreach (var cam in camraArray)
        {
            Destroy(cam.gameObject);
        }
        camraArray.Clear();
        for (int y = 0; y < viewCount.y; y++)
        {
            for (int x = 0; x < viewCount.x; x++)
            {
                var cam = Instantiate(singleViewCameraPrefab, transform);
                cam.transform.localPosition = new Vector3(x, y, 0);
                camraArray.Add(cam);
            }
        }
    }

    private void OnEnable()
    {
        SetupCameraArray();
    }
}

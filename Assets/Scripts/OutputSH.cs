using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OutputSH : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OutputSphericalHarmonics();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OutputSphericalHarmonics()
    {
        SphericalHarmonicsL2 sh = RenderSettings.ambientProbe;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Debug.Log($"SH[{i},{j}] = {sh[i, j]}");
            }
        }
    }
}

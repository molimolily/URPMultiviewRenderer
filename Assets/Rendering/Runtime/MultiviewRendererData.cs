using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[CreateAssetMenu(menuName = "Rendering/Multiview Renderer", fileName = "MultiviewRenderer")]
public class MultiviewRendererData : ScriptableRendererData
{
    protected override ScriptableRenderer Create()
    {
        return new MultiviewRenderer(this);
    }
}


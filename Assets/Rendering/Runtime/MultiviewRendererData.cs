using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    [CreateAssetMenu(menuName = "Rendering/Multiview Renderer", fileName = "MultiviewRenderer")]
    public class MultiviewRendererData : ScriptableRendererData
    {
        public Shader mergeShader;
        protected override ScriptableRenderer Create()
        {
            return new MultiviewRenderer(mergeShader, this);
        }
    }
}


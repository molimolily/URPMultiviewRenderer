using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MVR
{
    [CreateAssetMenu(menuName = "Rendering/Multiview Renderer", fileName = "MultiviewRenderer")]
    public class MultiviewRendererData : ScriptableRendererData
    {
        public Vector2Int viewCount = new Vector2Int(40, 20);
        public Vector2Int viewResolution = new Vector2Int(128, 128);
        public Shader mergeShder;
        protected override ScriptableRenderer Create()
        {
            viewCount.x = Mathf.Max(1, viewCount.x);
            viewCount.y = Mathf.Max(1, viewCount.y);

            viewResolution.x = Mathf.Max(1, viewResolution.x);
            viewResolution.y = Mathf.Max(1, viewResolution.y);

            return new MultiviewRenderer(viewCount, viewResolution, mergeShder, this);
        }
    }
}


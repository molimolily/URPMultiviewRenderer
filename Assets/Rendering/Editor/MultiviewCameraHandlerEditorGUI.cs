using System;
using UnityEditor;
using UnityEngine;

namespace MVR
{
    [CustomEditor(typeof(MultiviewCameraHandler))]
    public class MultiviewCameraHandlerEditorGUI : Editor
    {
        private bool shouldRender;
        private MultiviewCameraHandler handler;
        private int maxTextureArraySlices;
        void OnEnable()
        {
            maxTextureArraySlices = SystemInfo.supports2DArrayTextures ? SystemInfo.maxTextureArraySlices : 1;
            Debug.Log("Max Texture2DArray Slices: " + maxTextureArraySlices);
        }
        public override void OnInspectorGUI()
        {
            shouldRender = false;
            handler = target as MultiviewCameraHandler;
            if (handler == null) return;

            EditorGUI.BeginChangeCheck();
            var viewCount = EditorGUILayout.Vector2IntField("View Count", handler.ViewCount);
            if (EditorGUI.EndChangeCheck())
            {
                if(viewCount.x < 1 || viewCount.y < 1)
                {
                    // Debug.LogWarning("View Count must be greater than 0");
                }
                else if (viewCount.x * viewCount.y > maxTextureArraySlices)
                {
                    Debug.LogWarning("View Count exceeds the maximum number of texture array slices");
                }
                else if (viewCount.x > 0 && viewCount.y > 0)
                {
                    shouldRender = true;
                    Undo.RecordObject(handler, "Change View Count");
                    handler.ViewCount = viewCount;
                    EditorUtility.SetDirty(handler);
                }

            }

            EditorGUI.BeginChangeCheck();
            var multiviewCamera = EditorGUILayout.ObjectField("Multiview Camera", handler.multiviewCamera, typeof(BaseMultiviewCamera), true) as BaseMultiviewCamera;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(handler, "Change Multiview Camera");
                handler.multiviewCamera = multiviewCamera;
                EditorUtility.SetDirty(handler);
            }

            if(multiviewCamera != null) shouldRender = true;

            handler.ShouldRender = shouldRender;
        }
    }
}

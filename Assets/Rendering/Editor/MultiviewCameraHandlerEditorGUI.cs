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
            // Debug.Log("Max Texture2DArray Slices: " + maxTextureArraySlices);
        }
        public override void OnInspectorGUI()
        {
            shouldRender = false;
            handler = target as MultiviewCameraHandler;
            if (handler == null) return;

            EditorGUILayout.LabelField("ViewCount", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();
                // var viewCount = EditorGUILayout.Vector2IntField("View Count", handler.ViewCount);
                var viewCount = handler.ViewCount;
                var viewX = EditorGUILayout.DelayedIntField("x", viewCount.x);
                var viewY = EditorGUILayout.DelayedIntField("y", viewCount.y);
                if (EditorGUI.EndChangeCheck())
                {
                    if (viewX < 1 || viewY < 1)
                    {
                        // Debug.LogWarning("View Count must be greater than 0");
                    }
                    else if (viewX * viewY > maxTextureArraySlices)
                    {
                        Debug.LogWarning("View Count exceeds the maximum number of texture array slices");
                    }
                    else if (viewX > 0 && viewY > 0)
                    {
                        shouldRender = true;
                        Undo.RecordObject(handler, "Change View Count");
                        handler.ViewCount = new Vector2Int(viewX, viewY);
                        EditorUtility.SetDirty(handler);
                    }

                }
            }

            EditorGUILayout.Space();

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

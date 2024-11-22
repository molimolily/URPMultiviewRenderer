using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SingleViewCamera))]
public class SingleViewCameraEditor : Editor
{
    public enum ProjectionMode
    {
        Perspective,
        Frustum
    }

    private SingleViewCamera camera;
    private ProjectionMode projectionMode = ProjectionMode.Perspective;

    private readonly GUIContent nearContent = new GUIContent("Near", "Near clipping plane");
    private readonly GUIContent farContent = new GUIContent("Far", "Far clipping plane");
    private readonly GUIContent fovContent = new GUIContent("Field of View", "Field of view [deg]");
    private readonly GUIContent aspectContent = new GUIContent("Aspect", "Aspect ratio \n width/height");
    private readonly GUIContent leftContent = new GUIContent("Left", "Left value of frustum");
    private readonly GUIContent rightContent = new GUIContent("Right", "Right value of frustum");
    private readonly GUIContent topContent = new GUIContent("Top", "Top value of frustum");
    private readonly GUIContent bottomContent = new GUIContent("Bottom", "Bottom value of frustum");

    private void OnEnable()
    {
        camera = target as SingleViewCamera;
        if (camera == null) return;
        projectionMode = ProjectionMode.Perspective;

        camera.frustum.zNear = 0.3f;
        camera.frustum.zFar = 1000.0f;
        camera.FoV = 60.0f;
        camera.Aspect = 16.0f / 9.0f;
    }
    public override void OnInspectorGUI()
    {
        camera = target as SingleViewCamera;
        if (camera == null) return;

        EditorGUI.BeginChangeCheck();
        var pMode = (ProjectionMode)EditorGUILayout.EnumPopup("Projection Mode", projectionMode);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(camera, "Change Projection Mode");
            projectionMode = pMode;
            EditorUtility.SetDirty(camera);
        }

        switch (projectionMode)
        {
            case ProjectionMode.Perspective:
                DrawPerspectiveSettings();
                break;
            case ProjectionMode.Frustum:
                DrawFrustumSettings();
                break;
        }
    }

    void DrawPerspectiveSettings()
    {
        EditorGUILayout.LabelField("Perspective Settings", EditorStyles.boldLabel);

        using (new EditorGUI.IndentLevelScope())
        {
            
            EditorGUI.BeginChangeCheck();
            var near = EditorGUILayout.FloatField(nearContent, camera.frustum.zNear);
            if (EditorGUI.EndChangeCheck())
            {
                if(near > 0)
                {
                    Undo.RecordObject(camera, "Change Near");
                    camera.frustum.zNear = near;
                    EditorUtility.SetDirty(camera);
                }
            }

            EditorGUI.BeginChangeCheck();
            var far = EditorGUILayout.FloatField("Far", camera.frustum.zFar);
            if (EditorGUI.EndChangeCheck())
            {
                if(far > near)
                {
                    Undo.RecordObject(camera, "Change Far");
                    camera.frustum.zFar = far;
                    EditorUtility.SetDirty(camera);
                }
            }

            EditorGUI.BeginChangeCheck();
            var fov = EditorGUILayout.Slider("Field of View", camera.FoV, 0.1f, 179.0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(camera, "Change Field of View");
                camera.FoV = fov;
                EditorUtility.SetDirty(camera);
            }

            EditorGUI.BeginChangeCheck();
            var aspect = EditorGUILayout.FloatField("Aspect", camera.Aspect);
            if (EditorGUI.EndChangeCheck())
            {
                if (aspect > 0)
                {
                    Undo.RecordObject(camera, "Change Aspect");
                    camera.Aspect = aspect;
                    EditorUtility.SetDirty(camera);
                }
            }
        }
    }

    void DrawFrustumSettings()
    {
        EditorGUILayout.LabelField("Frustum Settings", EditorStyles.boldLabel);

        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUI.BeginChangeCheck();
            var near = EditorGUILayout.FloatField(nearContent, camera.frustum.zNear);
            if (EditorGUI.EndChangeCheck())
            {
                if (near > 0)
                {
                    Undo.RecordObject(camera, "Change Near");
                    camera.frustum.zNear = near;
                    EditorUtility.SetDirty(camera);
                }
            }

            EditorGUI.BeginChangeCheck();
            var far = EditorGUILayout.FloatField(farContent, camera.frustum.zFar);
            if (EditorGUI.EndChangeCheck())
            {
                if (far > near)
                {
                    Undo.RecordObject(camera, "Change Far");
                    camera.frustum.zFar = far;
                    EditorUtility.SetDirty(camera);
                }
            }

            EditorGUI.BeginChangeCheck();
            var left = EditorGUILayout.FloatField(leftContent, camera.frustum.left);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(camera, "Change Left");
                camera.frustum.left = left;
                EditorUtility.SetDirty(camera);
            }

            EditorGUI.BeginChangeCheck();
            var right = EditorGUILayout.FloatField(rightContent, camera.frustum.right);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(camera, "Change Right");
                camera.frustum.right = right;
                EditorUtility.SetDirty(camera);
            }

            EditorGUI.BeginChangeCheck();
            var top = EditorGUILayout.FloatField(topContent, camera.frustum.top);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(camera, "Change Top");
                camera.frustum.top = top;
                EditorUtility.SetDirty(camera);
            }

            EditorGUI.BeginChangeCheck();
            var bottom = EditorGUILayout.FloatField(bottomContent, camera.frustum.bottom);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(camera, "Change Bottom");
                camera.frustum.bottom = bottom;
                EditorUtility.SetDirty(camera);
            }
        }
    }
}

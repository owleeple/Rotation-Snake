using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BorderController))]
public class BorderControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BorderController controller = (BorderController)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Refresh Now"))
        {
            controller.RefreshBorderSettings();
        }

        EditorGUILayout.HelpBox("Changes are automatically applied in Edit Mode", MessageType.Info);
    }
}
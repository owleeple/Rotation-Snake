using UnityEditor;
using UnityEngine;

public class SplitBorderShaderEditor : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // ??????
        MaterialProperty useSplitColorProp = FindProperty("_UseSplitColor", properties);
        MaterialProperty colorAProp = FindProperty("_ColorA", properties);
        MaterialProperty colorBProp = FindProperty("_ColorB", properties);
        MaterialProperty directionProp = FindProperty("_Direction", properties);
        MaterialProperty ratioProp = FindProperty("_Ratio", properties);
        MaterialProperty useBorderProp = FindProperty("_UseBorder", properties);
        MaterialProperty borderColorProp = FindProperty("_BorderColor", properties);
        MaterialProperty borderWidthProp = FindProperty("_BorderWidth", properties);

        // ?????
        MaterialProperty showLeftProp = FindProperty("_ShowLeft", properties);
        MaterialProperty showRightProp = FindProperty("_ShowRight", properties);
        MaterialProperty showTopProp = FindProperty("_ShowTop", properties);
        MaterialProperty showBottomProp = FindProperty("_ShowBottom", properties);

        // ??????
        MaterialProperty showTopLeftProp = FindProperty("_ShowTopLeft", properties);
        MaterialProperty showTopRightProp = FindProperty("_ShowTopRight", properties);
        MaterialProperty showBottomLeftProp = FindProperty("_ShowBottomLeft", properties);
        MaterialProperty showBottomRightProp = FindProperty("_ShowBottomRight", properties);

        GUILayout.Label("Split Color Settings", EditorStyles.boldLabel);

        // Split color functionality
        bool useSplitColor = useSplitColorProp.floatValue > 0.5f;
        useSplitColor = EditorGUILayout.Toggle("Use Split Color", useSplitColor);
        useSplitColorProp.floatValue = useSplitColor ? 1 : 0;

        if (useSplitColor)
        {
            materialEditor.ShaderProperty(colorAProp, colorAProp.displayName);
            materialEditor.ShaderProperty(colorBProp, colorBProp.displayName);
            materialEditor.ShaderProperty(directionProp, directionProp.displayName);
            materialEditor.ShaderProperty(ratioProp, ratioProp.displayName);
        }
        else
        {
            materialEditor.ShaderProperty(colorAProp, colorAProp.displayName);
        }

        EditorGUILayout.Space();
        GUILayout.Label("Border Settings", EditorStyles.boldLabel);

        // Border functionality
        bool useBorder = useBorderProp.floatValue > 0.5f;
        useBorder = EditorGUILayout.Toggle("Use Border", useBorder);
        useBorderProp.floatValue = useBorder ? 1 : 0;

        if (useBorder)
        {
            materialEditor.ShaderProperty(borderColorProp, borderColorProp.displayName);
            materialEditor.ShaderProperty(borderWidthProp, borderWidthProp.displayName);

            EditorGUILayout.Space();
            GUILayout.Label("Edge Visibility", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(showLeftProp, showLeftProp.displayName);
            materialEditor.ShaderProperty(showRightProp, showRightProp.displayName);
            materialEditor.ShaderProperty(showTopProp, showTopProp.displayName);
            materialEditor.ShaderProperty(showBottomProp, showBottomProp.displayName);

            EditorGUILayout.Space();
            GUILayout.Label("Corner Visibility", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(showTopLeftProp, showTopLeftProp.displayName);
            materialEditor.ShaderProperty(showTopRightProp, showTopRightProp.displayName);
            materialEditor.ShaderProperty(showBottomLeftProp, showBottomLeftProp.displayName);
            materialEditor.ShaderProperty(showBottomRightProp, showBottomRightProp.displayName);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Edges and corners can be controlled independently", MessageType.Info);
    }
}
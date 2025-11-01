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
        MaterialProperty showLeftProp = FindProperty("_ShowLeft", properties);
        MaterialProperty showRightProp = FindProperty("_ShowRight", properties);
        MaterialProperty showTopProp = FindProperty("_ShowTop", properties);
        MaterialProperty showBottomProp = FindProperty("_ShowBottom", properties);

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

            EditorGUILayout.HelpBox("Color A and Color B create a gradient split based on direction and ratio. Applies to inner area only.", MessageType.Info);
        }
        else
        {
            materialEditor.ShaderProperty(colorAProp, colorAProp.displayName);
            EditorGUILayout.HelpBox("Using Color A as solid color for inner area.", MessageType.Info);
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
            GUILayout.Label("Border Visibility", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(showLeftProp, showLeftProp.displayName);
            materialEditor.ShaderProperty(showRightProp, showRightProp.displayName);
            materialEditor.ShaderProperty(showTopProp, showTopProp.displayName);
            materialEditor.ShaderProperty(showBottomProp, showBottomProp.displayName);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Border uses solid color. Split colors apply only to inner area.", MessageType.Info);
    }
}
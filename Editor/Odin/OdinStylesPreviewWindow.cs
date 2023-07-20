using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class OdinStylesPreviewWindow : EditorWindow
{
    private Vector2 _scrollPos;
    private Type[] _typesToDisplay = new Type[] { typeof(SirenixGUIStyles), typeof(EditorStyles) };
    private GUIStyle _labelStyle;

    private const int StyleWidth = 350;
    private const int LabelWidth = 200;

    private const int Margin = 10;
    private const int MinCellHeight = 40;
    private int _columnCount;

    [MenuItem("NCat Odin/Odin Styles Preview")]
    private static void ShowWindow()
    {
        GetWindow<OdinStylesPreviewWindow>("Odin Styles Preview");
    }

    private void OnEnable()
    {
        _labelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };
        _labelStyle.alignment = TextAnchor.MiddleRight;
    }

    private void OnGUI()
    {
        _columnCount = Mathf.Max(1, (int)((position.width - Margin) / (StyleWidth + Margin)));
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        foreach (var type in _typesToDisplay)
        {
            DrawTypeStyles(type);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTypeStyles(Type type)
    {
        GUILayout.BeginVertical(SirenixGUIStyles.BoxContainer);
        GUILayout.Label(type.Name, SirenixGUIStyles.SectionHeader);
        GUILayout.EndVertical();

        //GUILayout.Space(10);  // Increase row spacing
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
        var rowCount = Mathf.CeilToInt((float)properties.Length / _columnCount);

        for (int i = 0; i < rowCount; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < _columnCount; j++)
            {
                var index = i * _columnCount + j;
                if (index < properties.Length)
                {
                    DrawStyleProperty(properties[index]);
                    GUILayout.Space(Margin);  // Increase column spacing
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(Margin);  // Increase row spacing
        }
    }

    private void DrawStyleProperty(PropertyInfo property)
    {
        var style = property.GetValue(null, null) as GUIStyle;
        if (style != null)
        {
            GUILayout.BeginVertical(SirenixGUIStyles.BoxContainer, GUILayout.ExpandWidth(false), 
                GUILayout.Width(StyleWidth), GUILayout.MinHeight(MinCellHeight));
            GUILayout.BeginHorizontal();
            GUILayout.Label(property.Name + ":", _labelStyle, GUILayout.Width(LabelWidth));
            GUILayout.Label("Sample", style);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}

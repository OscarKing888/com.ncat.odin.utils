using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace NCat.Editor.Odin
{
    public class OdinEditorIconsPreviewWindow : OdinEditorWindow
    {
        private static Dictionary<string, EditorIcon> icons;

        [MenuItem("NCat Odin/Editor Icons Preview")]
        private static void OpenWindow()
        {
            GetWindow<OdinEditorIconsPreviewWindow>().Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            icons = new Dictionary<string, EditorIcon>();
            Type editorIconsType = typeof(EditorIcons);
            PropertyInfo[] fields = editorIconsType.GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.PropertyType == typeof(EditorIcon))
                {
                    icons[field.Name] = (EditorIcon)field.GetValue(null);
                }
            }
        }

        Vector2 viewVerticalScrollPos;

        protected override void OnGUI()
        {
            int columns = Mathf.Max(1, (int)(this.position.width / 300));

            viewVerticalScrollPos = EditorGUILayout.BeginScrollView(viewVerticalScrollPos);
            EditorGUILayout.BeginVertical();
            SirenixEditorGUI.BeginBox("");
            int currentColumn = 0;
            int idx = 1;
            foreach (var kvp in icons)
            {
                if (currentColumn % columns == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }


                EditorGUILayout.BeginHorizontal();
                if (SirenixEditorGUI.IconButton(kvp.Value, 32, 32))
                {
                    TextEditor editor = new TextEditor();
                    editor.text = kvp.Key;
                    editor.Copy();
                }
                GUILayout.Label($"{idx} - {kvp.Key}", GUILayout.Width(200));
                ++idx;
                EditorGUILayout.EndHorizontal();


                if ((currentColumn + 1) % columns == 0 || (currentColumn + 1) == icons.Count)
                {
                    EditorGUILayout.EndHorizontal();
                }

                currentColumn++;
            }
            SirenixEditorGUI.EndBox();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
} // namespace NCat.Editor.Odin
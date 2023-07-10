using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NCat.Editor.Odin
{
    public abstract class NCatScrollEditorWindow<T> : EditorWindow where T : EditorWindow
    {
        protected static T instance;

        protected static void ShowWindow(string title, float minW = 200f, float minH = 300f)
        {
            if (NCatScrollEditorWindow<T>.instance != null)
            {
                NCatScrollEditorWindow<T>.instance.Show(true);
                return;
            }

            T win = (T)EditorWindow.GetWindow(typeof(T), false, title, true);
            win.minSize = new Vector2(minW, minH);
            NCatScrollEditorWindow<T>.instance = win as T;
        }

        protected Vector2 ScrollPos = Vector2.zero;
        // Start is called before the first frame update
        void OnGUI()
        {
            Color clr = GUI.color;
            Color contentClr = GUI.contentColor;
            Color clrBg = GUI.backgroundColor;

            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
            EditorGUILayout.BeginVertical();
            OnDrawGUI();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUI.color = clr;
            GUI.contentColor = contentClr;
            GUI.backgroundColor = clrBg;
        }

        // Update is called once per frame
        protected abstract void OnDrawGUI();

        public static U ObjectField<U>(U obj, string label) where U : UnityEngine.Object
        {
            return (U)EditorGUILayout.ObjectField(label, obj, typeof(U), true);
        }
    }
}
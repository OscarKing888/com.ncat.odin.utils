using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace NCat.Editor.Odin
{
    public class NCatEnumCreateToolPage : ScriptableSingleton<NCatEnumCreateToolPage>
    {

#if UNITY_EDITOR
        [LabelText("资源路径")]
        public string AssetRoot = "Assets/Editor/";

        [LabelText("菜单显示名")]
        public string MenuName = "枚举显示名";

        [LabelText("enum 类型名")]
        public string GenEnumName;


        [Button("创建新枚举", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 1)]
        public void CreateNewEnum()
        {
            string file = $"{AssetRoot}Enum/{GenEnumName}.asset";

            if (File.Exists(file))
            {
                EditorUtility.DisplayDialog("Error", $"已经存在枚举：{file}", "OK");
                return;
            }

            //AssetDatabase.CreateFolder(AssetRoot, "Enums");

            string dir = System.IO.Path.Combine(
                Directory.GetCurrentDirectory(),
                System.IO.Path.GetDirectoryName(file));
            Debug.Log("Create Dir:" + dir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string assetPath = file;

            NCatEnumDefine newDef = ScriptableObject.CreateInstance<NCatEnumDefine>();
            newDef.MenuName = MenuName;
            newDef.GenEnumName = GenEnumName;

            AssetDatabase.CreateAsset(newDef, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


#endif
    }

}
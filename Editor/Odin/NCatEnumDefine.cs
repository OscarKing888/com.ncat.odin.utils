using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NCat.Editor.Odin
{

    [Serializable]
    public class NCatEnumDefineData
    {
        [HideLabel]
        [VerticalGroup("枚举名")]
        public string Name;

        [HideLabel]
        [VerticalGroup("编辑器显示名")]
        public string DisplayName;
    }


    [CreateAssetMenu(fileName = "EnumDefine", menuName = "NCatTools/创建枚举配置")]
    public class NCatEnumDefine : ScriptableObject
    {
        [LabelText("枚举定义列表")]
        [InfoBox("点击右边的 + 号新增")]
        [ListDrawerSettings(ShowIndexLabels = false, ShowPaging = false, ShowItemCount = true, DraggableItems = true)]
        [TableList(AlwaysExpanded = true)]
        public List<NCatEnumDefineData> DefineListNew = new List<NCatEnumDefineData>();


#if UNITY_EDITOR
        [LabelText("菜单显示名")]
        public string MenuName = "Define";

        [LabelText("enum 类型名")]
        public string GenEnumName;

        [LabelText("enum 显示名")]
        public string GenEnumDisplayName;

        [LabelText("enum 变量前辍")]
        public string GenEnumPrefix;

        [LabelText("是否为Mask型")]
        public bool IsMask;

        [LabelText("自动生成None变量")]
        public bool GenNoneItem = true;

        [LabelText("自动生成Max变量")]
        public bool GenMaxItem = true;

        [LabelText("开始变量数值")]
        public int StartItemValue = 1;

        [LabelText("代码生成目录")]
        public string GenFolder = "/_AutoGenScripts/";



        // [TitleGroup("", Order = -100)]
        [ButtonGroup(Order = -100)]
        [Button("生成代码", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 1)]
        public void GenCodeFile()
        {
            string str = Code;

            string d = Application.dataPath + GenFolder;
            string saveFile = d + GenEnumName + ".cs";
            if (!System.IO.Directory.Exists(d))
            {
                System.IO.Directory.CreateDirectory(d);
            }
            System.IO.File.WriteAllText(saveFile, str);

            AssetDatabase.Refresh();
        }

        // [TitleGroup("", Order = -100)]
        [ButtonGroup(Order = -100)]
        [Button("保存", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 1)]
        public void SaveAssets()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // [TitleGroup("", Order = -100)]
        [ButtonGroup(Order = -100)]
        [Button("复制变量列表", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 1)]
        public void CopyNameList()
        {
            string tmpStr = "";
            for (int i = 0; i < DefineListNew.Count; i++)
            {
                NCatEnumDefineData data = DefineListNew[i];
                if (!string.IsNullOrEmpty(data.Name))
                {
                    tmpStr += data.Name + "\r\n";
                }
            }

            CopyToClipboard(tmpStr);
        }

        // [TitleGroup("", Order = -100)]
        [ButtonGroup(Order = -100)]
        [Button("复制描述列表", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 1)]
        public void CopyDescList()
        {
            string tmpStr = "";
            for (int i = 0; i < DefineListNew.Count; i++)
            {
                NCatEnumDefineData data = DefineListNew[i];
                if (!string.IsNullOrEmpty(data.Name))
                {
                    tmpStr += data.DisplayName + "\r\n";
                }
            }

            CopyToClipboard(tmpStr);
        }

        public static void CopyToClipboard(string str)
        {
            GUIUtility.systemCopyBuffer = str;
        }


        [InfoBox("@Code")]
        [ReadOnly]
        [ShowInInspector]
        public string Code
        {
            get
            {
                string str = "//\r\n";
                str += "// Auto generated code, DON NOT modify by manual!!!\r\n";
                str += "//\r\n";
                str += "\r\n";
                str += "\r\n";
                str += "using Sirenix.OdinInspector;\r\n\r\n";

                if (IsMask)
                {
                    str += "[System.Flags]\r\n";
                }

                if(!string.IsNullOrEmpty(GenEnumDisplayName))
                {
                    str += $"[LabelText(\"{GenEnumDisplayName}\")]\r\n";
                }

                str += "public enum " + GenEnumName + "\r\n{\r\n";

                if (GenNoneItem)
                {
                    str += $"\t[LabelText(\"无\")]\r\n";
                    str += "\t" + GenEnumPrefix + "None = 0,\r\n\r\n";
                }

                int maxCnt = 0;

                string AllMask = "";

                for (int i = 0; i < DefineListNew.Count; i++)
                {
                    NCatEnumDefineData data = DefineListNew[i];
                    if (!string.IsNullOrEmpty(data.Name))
                    {
                        string dispName = string.IsNullOrEmpty(data.DisplayName) ? data.Name : data.DisplayName;
                        str += $"\t[LabelText(\"{dispName}\")]\r\n";
                        if (IsMask)
                        {
                            AllMask += $"{GenEnumPrefix}{data.Name}";
                            if (i < DefineListNew.Count - 1)
                            {
                                AllMask += " | ";
                            }

                            str += $"\t{GenEnumPrefix}{data.Name} = 1 << {i + 1},\r\n\r\n";
                        }
                        else
                        {
                            str += $"\t{GenEnumPrefix}{data.Name} = {i + StartItemValue},\r\n\r\n";
                        }

                        ++maxCnt;
                    }
                }

                if (IsMask)
                {
                    str += $"\t{GenEnumPrefix}All = {AllMask}\r\n";
                }
                else if (GenMaxItem)
                {
                    str += "\t" + GenEnumPrefix + "Max = " + ++maxCnt + "\r\n";
                }

                str += "}";

                return str;
            }
        }
#endif
    }

}
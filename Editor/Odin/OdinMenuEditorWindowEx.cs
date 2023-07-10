using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NCat.Editor.Odin
{
    public class OdinMenuEditorPanel : SerializedScriptableObject
    {
        public virtual void OnClose()
        {

        }
    }

    public class OdinMenuEditorWindowEx<T> : OdinMenuEditorWindow
    {
        public class PanelInfo
        {
            public string Title;
            public Type PanelType;
            public EditorIcon Icon;
            public OdinMenuEditorPanel Instance;
        }

        protected List<PanelInfo> PanelList = new List<PanelInfo>();

        public OdinMenuEditorWindowEx()
        {
            this.OnClose += OdinMenuEditorWindowEx_OnClose;
        }

        private void OdinMenuEditorWindowEx_OnClose()
        {
            for (int i = 0; i < PanelList.Count; i++)
            {
                var panelInfo = PanelList[i];
                if (panelInfo.Instance != null)
                {
                    panelInfo.Instance.OnClose();
                }
            }
        }

        protected virtual void OnInitPanelList()
        {
        }


        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, OdinMenuStyle.TreeViewStyle);
            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle.Height = 32;
            tree.DefaultMenuStyle.IconSize = 20;

            //Debug.Log("======= BuildMenuTree");
            if (PanelList.Count == 0)
            {
                OnInitPanelList();
            }

            for (int i = 0; i < PanelList.Count; i++)
            {
                var panelInfo = PanelList[i];
                if (panelInfo.Instance == null)
                {
                    //Debug.LogFormat("======= Create panel:{0}", panelInfo.Title);
                    panelInfo.Instance = ScriptableObject.CreateInstance(panelInfo.PanelType) as OdinMenuEditorPanel;
                }

                tree.Add(panelInfo.Title, panelInfo.Instance, panelInfo.Icon);
            }

            return tree;
        }

        public OdinMenuStyle ReturnOdinMenuStyle()
        {
            OdinMenuStyle odinMenuStyle = new OdinMenuStyle()
            {
                Height = 50,
                Offset = 20.00f,
                IndentAmount = 15.00f,
                IconSize = 20,
                IconOffset = 0.00f,
                NotSelectedIconAlpha = 0.85f,
                IconPadding = 0.00f,
                TriangleSize = 16.00f,
                TrianglePadding = 0.00f,
                AlignTriangleLeft = true,
                Borders = true,
                BorderPadding = 0.00f,
                BorderAlpha = 0.32f,
                SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
                SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
            };
            return odinMenuStyle;
        }
    }

} // namespace NCat.Editor.Odin
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetReference", menuName = "NCat/资源依赖表", order = 1)]
[System.Serializable]
public class AssetDependenciesTable : NCat.Editor.Odin.OdinMenuEditorPanel
{
    [InlineProperty]
    [Searchable]
    [LabelText("资源依赖表")]
    [ListDrawerSettings(ShowPaging = false, ShowIndexLabels = true)]
    public Dictionary<string, List<string>> DependenciesTable = new Dictionary<string, List<string>>();


    [InlineProperty]
    [Searchable]
    [LabelText("资源引用表")]
    [ListDrawerSettings(ShowPaging = true, ShowIndexLabels = true)]
    public Dictionary<string, List<string>> ReferenceTable = new Dictionary<string, List<string>>();

    public List<string> GetReferencedByAssets(string assetPath)
    {
        List<string> retLst = new List<string>();
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if(ReferenceTable.ContainsKey(guid))
        {
            var lst = ReferenceTable[guid];
            for (int i = 0; i < lst.Count; i++)
            {
                string pth = AssetDatabase.GUIDToAssetPath(lst[i]);
                retLst.Add(pth);
            }
        }

        return retLst;
    }
}


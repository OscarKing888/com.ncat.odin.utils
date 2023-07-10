
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct OptionItem
{
    //[ReadOnly]
    [HideLabel]
    public string Symbol;

    //[HorizontalGroup]
    [TableColumnWidth(60, Resizable = false), HideLabel, VerticalGroup("Enable")]
    public bool Enable;
}

[CreateAssetMenu(fileName = "NCatBuildConfig", menuName = "NCat/Build Config", order = 1)]
[System.Serializable]
public class SymbolDefineOptionItem : UnityEngine.ScriptableObject
{
    [TableList(ShowPaging = false)]
    [InlineProperty]
    public List<OptionItem> Options = new List<OptionItem>();
}

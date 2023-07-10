using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[Serializable]
public class DefineSymbolProxy
{
    public DefineSymbolProxy(string k)
    {
        SymbolDefine = k;
    }

    [TableColumnWidth(40), VerticalGroup("Symbol"), HideLabel, ReadOnly]
    public string SymbolDefine = "";

    [TableColumnWidth(60, Resizable = false), VerticalGroup("Actions"), Button("×")]
    protected virtual void Remove()
    {
        NCatBuildTool.RemoveCompileDefine(SymbolDefine);
    }
}

/// <summary>
/// NCatBuildTool
/// </summary>
public class NCatBuildTool : OdinEditorWindow
{
    [MenuItem("构建工具/Build...")]
    public static void ShowWindow()
    {
        var window = GetWindow<NCatBuildTool>("Build Options");
        //window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 800);
    }

    [ShowInInspector]
    [TitleGroup("Custom Options")]
    [LabelText("Build Options")]
    //[TableList(ShowPaging = false)]
    [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    //[InlineProperty]
    static SymbolDefineOptionItem buildOptions;


    [ShowInInspector]
    [TitleGroup("Editor Symbols")]
    [TableList(ShowPaging = false), LabelText("Scripting Define Symbols")]
    //[InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    static public List<DefineSymbolProxy> ScriptSymbolDefines = new List<DefineSymbolProxy>();


    [TitleGroup("Paths")]
    [ShowInInspector]
    public string AppDataPath
    {
        get { return Application.dataPath; }
    }

    [TitleGroup("Paths")]
    [ShowInInspector]
    public static string APKBuildPath = "APK";

    [TitleGroup("Paths")]
    [ShowInInspector]
    public static string AndroidExportPath = "AndroidProject";

    [TitleGroup("Paths")]
    [ShowInInspector]
    public static string IOSBuildPath = "IOS";


    [ShowInInspector]
    [TitleGroup("Paths")]
    public static string ProjectName
    {
        get
        {
            string str = PlayerSettings.productName;
            str = str.Replace(" ", "");
            return str;
        }
    }

    [ShowInInspector]
    [TitleGroup("Paths"), LabelText("APK Build Path")]
    public string ShowBuildAPKPath
    {
        get { return GetBuildAPKPath(); }
    }

    [ShowInInspector, ReadOnly]
    [TitleGroup("Paths"), LabelText("APK Dev Build Path")]
    public string ShowBuildAPKPathDev
    {
        get { return GetBuildAPKPath(true); }
    }



    [ShowInInspector]
    [TitleGroup("Paths"), LabelText("IOS Build Path")]
    public static string ShowBuildIOSPath
    {
        get { return GetBuildIOSPath(); }
    }


    [ShowInInspector]
    [TitleGroup("Paths"), LabelText("IOS Dev Build Path")]
    public static string ShowBuildIOSPathDev
    {
        get { return GetBuildIOSPath(true); }
    }

    [OnInspectorInit]
    void Init()
    {
        RefresFromDefines();
    }

    bool UpdateDefineSymbol(string defineStr, bool add, ref List<string> symbols)
    {
        if (add)
        {
            if (symbols.Contains(defineStr))
            {
                return false;
            }
            else
            {
                symbols.Add(defineStr);
                return true;
            }
        }
        else
        {
            if (symbols.Contains(defineStr))
            {
                symbols.Remove(defineStr);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    static bool HasDefineSymbol(string defineStr, List<string> symbols)
    {
        return symbols.Contains(defineStr);
    }



    // const string BuildOptionsAssetPath = NCatGameFramework.AssetRoot + "Assets/Editor/Resources/NCatBuildConfig.asset";
    const string BuildOptionsAssetPath = "Assets/Editor/Resources/NCatBuildConfig.asset";
    static SymbolDefineOptionItem LoadBuildOptions()
    {
        if (buildOptions == null)
        {
            buildOptions = AssetTools.LoadConfigAsset<SymbolDefineOptionItem>(BuildOptionsAssetPath);
        }

        return buildOptions;
    }

    [TitleGroup("Editor Symbols")]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0f)]
    static void RefresFromDefines()
    {
        string Settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> symbols = new List<string>(Settings.Split(';'));
        symbols.Sort();

        LoadBuildOptions();

        bool needSaveAssets = false;
        for (int i = 0; i < buildOptions.Options.Count; i++)
        {
            bool on = HasDefineSymbol(buildOptions.Options[i].Symbol, symbols);
            if (buildOptions.Options[i].Enable != on)
            {
                OptionItem itm = buildOptions.Options[i];
                itm.Enable = on;
                buildOptions.Options[i] = itm;
                needSaveAssets = true;
            }
        }

        ScriptSymbolDefines.Clear();
        foreach (var item in symbols)
        {
            var p = new DefineSymbolProxy(item);
            ScriptSymbolDefines.Add(p);
        }

        if (needSaveAssets)
        {
            AssetDatabase.SaveAssets();
        }
    }


    [TitleGroup("Editor Symbols")]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0f)]
    public void UpdateDefines()
    {
        if (buildOptions != null)
        {
            bool HasChanged = false;

            string Settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> symbols = new List<string>(Settings.Split(';'));
            symbols.Sort();

            for (int i = 0; i < buildOptions.Options.Count; i++)
            {
                HasChanged |= UpdateDefineSymbol(buildOptions.Options[i].Symbol, buildOptions.Options[i].Enable, ref symbols);
            }


            if (HasChanged)
            {
                try
                {
                    Settings = string.Empty;
                    for (int i = 0, imax = symbols.Count; i < imax; ++i)
                    {
                        if (i > 0) Settings += ";";
                        Settings += symbols[i];
                    }
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, Settings);
                    AssetDatabase.SaveAssets();
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("Error", e.ToString(), "OK");
                }

                RefresFromDefines();
            }
        }
    }

    static private bool IsBuildTargetSupported(BuildTargetGroup targetGroup, BuildTarget target)
    {
#if UNITY_2018_1_OR_NEWER
        return BuildPipeline.IsBuildTargetSupported(targetGroup, target);
#else
        MethodInfo isBuildTargetSupported = typeof(BuildPipeline).GetMethod("IsBuildTargetSupported", BindingFlags.Public| BindingFlags.NonPublic);
        return Convert.ToBoolean(isBuildTargetSupported.Invoke(null, new object[] { targetGroup, target }));
#endif
    }

    /// <summary>
    /// Attempts to remove a #define constant from the Player Settings
    /// </summary>
    /// <param name="defineCompileConstant">define constant</param>
    /// <param name="targets">platforms to add this for (default will add to all platforms)</param>

    static public void RemoveCompileDefine(string defineCompileConstant, params BuildTarget[] targetGroups)
    {
        if (targetGroups.Length == 0)
            targetGroups = (BuildTarget[])Enum.GetValues(typeof(BuildTarget));

        foreach (BuildTarget target in targetGroups)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            if (!IsBuildTargetSupported(targetGroup, target))
            {
                continue;
            }

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            int index = defines.IndexOf(defineCompileConstant, StringComparison.CurrentCulture);
            if (index < 0)
                continue;           //this target does not contain the define
            else if (index > 0)
                index -= 1;         //include the semicolon before the define
                                    //else we will remove the semicolon after the define

            //Remove the word and it's semicolon, or just the word (if listed last in defines)
            int lengthToRemove = Mathf.Min(defineCompileConstant.Length + 1, defines.Length - index);

            //remove the constant and it's associated semicolon (if necessary)
            defines = defines.Remove(index, lengthToRemove);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }

        var opts = LoadBuildOptions();

        bool needSaveAssets = false;
        

        bool found = false;

        for (int i = 0; i < opts.Options.Count; i++)
        {
            if (opts.Options[i].Symbol == defineCompileConstant)
            {
                found = true;
                OptionItem itm = opts.Options[i];
                itm.Enable = false;
                opts.Options[i] = itm;
                needSaveAssets = true;
            }
        }

        if (!found)
        {
            var item = new OptionItem();
            item.Symbol = defineCompileConstant;
            item.Enable = false;
            opts.Options.Add(item);
            EditorUtility.SetDirty(opts);
        }

        if (needSaveAssets)
        {
            AssetDatabase.SaveAssets();
        }

        RefresFromDefines();
    }

#if false
    void OnGUI2()
    {
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
        EditorGUILayout.BeginVertical();

        LoadBuildOptions();

        Color defaultColor = GUI.color;
        if (EditorTools.DrawHeader("Build Config"))
        {
            if (buildOptions != null)
            {
                for (int i = 0; i < buildOptions.Options.Count; i++)
                {
                    if (buildOptions.Options[i].Key == "-")
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                    else
                    {
                        OptionItem opt = buildOptions.Options[i];
                        opt.Enable = EditorGUILayout.ToggleLeft(buildOptions.Options[i].Key, buildOptions.Options[i].Enable);
                        buildOptions.Options[i] = opt;
                    }
                }
            }


            EditorUserBuildSettings.development = EditorGUILayout.ToggleLeft("Development Build", EditorUserBuildSettings.development);

            GUILayout.Space(32);

            string Settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            bool HasChanged = false;
            List<string> symbols = new List<string>(Settings.Split(';'));
            symbols.Sort();

            for (int i = 0, imax = symbols.Count; i < imax; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(symbols[i]);
                if(GUILayout.Button("x"))
                {
                    RemoveCompileDefine(symbols[i]);
                }
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = Color.green;
            if (GUILayout.Button("Refresh Defines", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
            {
                RefresFromDefines();
            }

            if (GUILayout.Button("Update Defines", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
            {
                if(buildOptions != null)
                {
                    for (int i = 0; i < buildOptions.Options.Count; i++)
                    {
                        HasChanged |= UpdateDefineSymbol(buildOptions.Options[i].Key, buildOptions.Options[i].Enable, ref symbols);
                    }


                    if (HasChanged)
                    {
                        try
                        {
                            Settings = string.Empty;
                            for (int i = 0, imax = symbols.Count; i < imax; ++i)
                            {
                                if (i > 0) Settings += ";";
                                Settings += symbols[i];
                            }
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, Settings);
                            AssetDatabase.SaveAssets();
                        }
                        catch (System.Exception e)
                        {
                            EditorUtility.DisplayDialog("Error", e.ToString(), "OK");
                        }
                    }
                }
            }
        }


        GUI.color = defaultColor;

        if (EditorTools.DrawHeader("Paths"))
        {
            EditorGUILayout.TextField(Application.dataPath);
            EditorGUILayout.TextField(APKBuildPath);
            EditorGUILayout.LabelField(GetBuildAPKPath());

            EditorGUILayout.TextField(AndroidExportPath);
            EditorGUILayout.LabelField(GetBuildAndroidExportPath());

            EditorGUILayout.TextField(IOSBuildPath);
            EditorGUILayout.LabelField(GetBuildIOSPath());
        }

        GUI.color = Color.yellow;
        if (GUILayout.Button("Build IOS", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
        {
            NCatBuildTool.BuildIOS();
        }

        if (GUILayout.Button("Build IOS Development", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
        {
            NCatBuildTool.BuildIOSDevelopment();
        }

        GUI.color = defaultColor;

        if (GUILayout.Button("Build APK"))
        {
            NCatBuildTool.BuildAPK();
        }

        if (GUILayout.Button("Build APK Development"))
        {
            NCatBuildTool.BuildAPKDevelopment();
        }


        if (GUILayout.Button("Export Android Project"))
        {
            NCatBuildTool.BuildExportAndroidProject();
        }

        if (GUILayout.Button("Export Android Project Development"))
        {
            NCatBuildTool.BuildExportAndroidProjectDevelopment();
        }


        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

#endif

    static public string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;

            if (e.enabled)
                names.Add(e.path);
        }

        return names.ToArray();
    }


    static public string GetBuildAPKPath(bool isDevelopment = false)
    {
        string val = Application.dataPath + "/../../" + ProjectName + "Build/" + NCatBuildTool.APKBuildPath;
        if (isDevelopment)
        {
            val += "Dev";
        }

        CreateDir(val);
        return val;
    }


    static public string GetBuildAndroidExportPath(bool isDevelopment = false)
    {
        string val = Application.dataPath + "/../../" + ProjectName + "Build/" + NCatBuildTool.AndroidExportPath;
        if (isDevelopment)
        {
            val += "Dev";
        }

        CreateDir(val);
        return val;
    }


    static public string GetBuildIOSPath(bool isDevelopment = false)
    {
        string val = Application.dataPath + "/../../" + ProjectName + "Build/" + NCatBuildTool.IOSBuildPath;
        if (isDevelopment)
        {
            val += "Dev";
        }

        CreateDir(val);
        return val;
    }

    static public void CreateDir(string val)
    {
        val = val.Replace(":", "_");

        System.IO.Directory.CreateDirectory(val);
    }


    static public BuildPlayerOptions GetGeneralAndroidBuildOptions()
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.target = BuildTarget.Android;
        options.targetGroup = BuildTargetGroup.Android;
        options.scenes = GetBuildScenes();

        return options;
    }



    [MenuItem("构建工具/Build APK")]
    [Button("Build APK", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [TitleGroup("Android")]
    public static void BuildAPK()
    {
        Debug.Log("====[Build] Build APK to:" + NCatBuildTool.GetBuildAPKPath());

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        BuildPlayerOptions options = NCatBuildTool.GetGeneralAndroidBuildOptions();
        options.locationPathName = NCatBuildTool.GetBuildAPKPath();
        BuildPipeline.BuildPlayer(options);

        EditorUtility.RevealInFinder(options.locationPathName);
    }



    [MenuItem("构建工具/Build APK Development")]
    [Button("Build APK Development", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [TitleGroup("Android")]
    public static void BuildAPKDevelopment()
    {
        Debug.Log("====[Build] Build APK Development to:" + NCatBuildTool.GetBuildAPKPath(true));

        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        BuildPlayerOptions options = NCatBuildTool.GetGeneralAndroidBuildOptions();
        options.locationPathName = NCatBuildTool.GetBuildAPKPath(true);
        options.options |= BuildOptions.Development;
        BuildPipeline.BuildPlayer(options);

        EditorUtility.RevealInFinder(options.locationPathName);
    }



    [MenuItem("构建工具/Export Android Project")]
    [Button("Export Android Project", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [TitleGroup("Android Projects")]
    public static void BuildExportAndroidProject()
    {
        Debug.Log("====[Build] Export Android Project to:" + NCatBuildTool.GetBuildAndroidExportPath());

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        BuildPlayerOptions options = NCatBuildTool.GetGeneralAndroidBuildOptions();
        options.locationPathName = NCatBuildTool.GetBuildAndroidExportPath();
        options.options |= BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(options);

        EditorUtility.RevealInFinder(options.locationPathName);
    }



    [MenuItem("构建工具/Export Android Project Development")]
    [Button("Export Android Project Development", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [TitleGroup("Android Projects")]
    public static void BuildExportAndroidProjectDevelopment()
    {
        Debug.Log("====[Build] Export Android Project Development to:" + NCatBuildTool.GetBuildAndroidExportPath(true));

        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        BuildPlayerOptions options = NCatBuildTool.GetGeneralAndroidBuildOptions();
        options.locationPathName = NCatBuildTool.GetBuildAndroidExportPath(true);
        options.options |= BuildOptions.Development | BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(options);

        EditorUtility.RevealInFinder(options.locationPathName);
    }



    [MenuItem("构建工具/Build IOS")]
    [Button("Build IOS", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [TitleGroup("IOS")]
    public static void BuildIOS()
    {
        Debug.Log("====[Build] Build ios development to:" + NCatBuildTool.GetBuildIOSPath());

        EditorUserBuildSettings.development = false;

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.target = BuildTarget.iOS;
        options.targetGroup = BuildTargetGroup.iOS;
        options.scenes = NCatBuildTool.GetBuildScenes();
        options.locationPathName = NCatBuildTool.GetBuildIOSPath();
        options.options = BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(options);

        EditorUtility.RevealInFinder(options.locationPathName);
    }

    [MenuItem("构建工具/Build IOS Development")]
    [Button("Build IOS Development", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [TitleGroup("IOS")]
    public static void BuildIOSDevelopment()
    {
        Debug.Log("====[Build] Build ios to:" + NCatBuildTool.GetBuildIOSPath(true));

        EditorUserBuildSettings.development = true;

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.target = BuildTarget.iOS;
        options.targetGroup = BuildTargetGroup.iOS;
        options.scenes = NCatBuildTool.GetBuildScenes();
        options.locationPathName = NCatBuildTool.GetBuildIOSPath(true);
        options.options = BuildOptions.Development | BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(options);

        EditorUtility.RevealInFinder(options.locationPathName);
    }
}

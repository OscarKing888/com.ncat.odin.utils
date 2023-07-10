using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;





public static class AssetTools
{
    public struct FMD5Result
    {
        public long Size;
        public string MD5Val;
    }

    public static FMD5Result GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = File.OpenRead(fileName);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            string md5Val = BitConverter.ToString(retVal).ToUpper().Replace("-", "");
            return new FMD5Result()
            {
                MD5Val = md5Val,
                Size = new FileInfo(fileName).Length
            };
        }
        catch (Exception)
        {
            throw;
        }
    }
    
    
    public static void RefreshDependenciesList(AssetDependenciesTable referenceTree)
    {
        referenceTree.DependenciesTable.Clear();

        var files = AssetDatabase.FindAssets("");
        int procLen = files.Length;
        for (int i = 0; i < procLen; i++)
        {
            string guidStr = files[i];

            GUID guid;
            
            if(!GUID.TryParse(guidStr, out guid))
            {
                Debug.LogErrorFormat("-- parse guid failed:{0}", guidStr);
            }

            List<string> lst = new List<string>();
            referenceTree.DependenciesTable.Add(guidStr, lst);

            float p = (float)(i) / (float)procLen;
            EditorUtility.DisplayProgressBar("资源依赖", string.Format("正在生成资源依赖表:{0}/{1}", i + 1, procLen), p);
            GetDependencies(guidStr, lst);
        }
    }


    static void GetDependencies(string guid, List<string> lst)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

        var deps = AssetDatabase.GetDependencies(assetPath, true);
        for (int i = 0; i < deps.Length; i++)
        {
            var depGuid = AssetDatabase.GUIDFromAssetPath(deps[i]).ToString();
            lst.Add(depGuid);
        }
    }



    public static void CreateReferenceTable(AssetDependenciesTable referenceTree, Dictionary<string, List<string>> table)
    {
        table.Clear();
        
        int keyCnt = referenceTree.DependenciesTable.Keys.Count;
        var keys = new string[keyCnt];
        referenceTree.DependenciesTable.Keys.CopyTo(keys, 0);


        var vals = new List<string>[keyCnt];
        referenceTree.DependenciesTable.Values.CopyTo(vals, 0);

        for (int i = 0; i < keyCnt; i++)
        {
            float p = (float)(i) / (float)keyCnt;
            EditorUtility.DisplayProgressBar("资源引用表", string.Format("正在生成资源引用表:{0}/{1}", i + 1, keyCnt), p);

            var key = keys[i];
            //var keyDeps = vals[i];

            for (int j = 0; j < keyCnt; j++)
            {
                var val = vals[j];
                for (int k = 0; k < val.Count; k++)
                {
                    var valKey = val[k];
                    if(valKey == key)
                    {
                        var refKey = keys[j];
                        if(key != refKey)
                        {
                            if (!table.ContainsKey(refKey))
                            {
                                table.Add(refKey, new List<string>());
                            }

                            if (!table[refKey].Contains(key))
                            {
                                table[refKey].Add(key);
                            }
                            break;
                        }
                    }                    
                }
            }
        }

        EditorUtility.ClearProgressBar();
    }


    public static T LoadConfigAsset<T>(string assetPath)
        where T : ScriptableObject
    {
        T cfg = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        Debug.LogFormat("====[LoadConfigAsset] Load {0} {1}", cfg, assetPath);

        if (cfg == null)
        {
            Debug.LogWarningFormat("====[LoadConfigAsset] +++ Create Asset file:{0}", assetPath);
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            cfg = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
        return cfg;
    }

    public static AssetDependenciesTable GetDependenciesTable()
    {
        //const string cfgAssetPath = NCatGameFramework.AssetRoot + "Assets/Editor/Resources/AssetReference.asset";
        const string cfgAssetPath = "Assets/Editor/Resources/AssetReference.asset";
        return LoadConfigAsset<AssetDependenciesTable>(cfgAssetPath);
    }
}

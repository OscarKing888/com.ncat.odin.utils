using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class NCatBatchAssetTool
{
    public delegate bool ProcPrefabFunc(GameObject prefabInstance, string filePath);
    public delegate void ProcPrefabProgress(string title, string filePath, GameObject procObject, float progress);

    public delegate bool ProcAssetFunc<T>(T assetObj, string filePath);
    public delegate void ProcAssetProgress(string title, string filePath, UnityEngine.Object assetObj, float progress);



    /// <summary>
    /// ProcessPrefabs
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="typeFilter"></param>
    /// <param name="searchInFolders"></param>
    /// <param name="procTitle"></param>
    /// <param name="progress"></param>
    public static void ProcessPrefabs(ProcPrefabFunc proc, string typeFilter, string[] searchInFolders, string procTitle = "Process Prefabs", ProcPrefabProgress progress = null)
    {
        bool dirty = false;
        string[] files = AssetDatabase.FindAssets(typeFilter, searchInFolders);
        int procLen = files.Length;
        for (int i = 0; i < procLen; i++)
        {
            string guid = files[i];
            string filePath = AssetDatabase.GUIDToAssetPath(guid);

            //GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
            //this will failed to instantiate
            // GameObject prefab = PrefabUtility.LoadPrefabContents(filePath);
            //Debug.LogFormat(prefab, "[ProcessPrefabs] prefab:{0}", filePath);
            //if (prefab)
            {
                //GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                GameObject prefabInstance = PrefabUtility.LoadPrefabContents(filePath);


                if (progress != null)
                {
                    progress(procTitle, filePath, prefabInstance, (float)i / procLen);
                }


                if (proc(prefabInstance, filePath))
                {
                    dirty = true;

                    bool ok = false;
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, filePath, out ok);
                    if (!ok)
                    {
                        Debug.LogErrorFormat(prefabInstance, "[ProcessPrefabs] {0}Save to prefab failed:{0}", procTitle, filePath);
                    }
                }

                UnityEngine.Object.DestroyImmediate(prefabInstance);
            }
            //             else
            //             {
            //                 Debug.LogErrorFormat("[ProcessPrefabs] prefab failed to load:{0}", filePath);
            //                    }
        }

        if (dirty)
        {
            AssetDatabase.SaveAssets();
        }

        EditorUtility.ClearProgressBar();
    }



    /// <summary>
    /// ProcessAssets
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="typeFilter"></param>
    /// <param name="searchInFolders"></param>
    /// <param name="procTitle"></param>
    /// <param name="progress"></param>
    public static void ProcessAssets<T>(ProcAssetFunc<T> proc, string typeFilter, string[] searchInFolders, string procTitle = "Process Asset Objects", ProcAssetProgress progress = null)
        where T : UnityEngine.Object
    {
        bool dirty = false;
        string[] files = AssetDatabase.FindAssets(typeFilter, searchInFolders);
        int procLen = files.Length;
        for (int i = 0; i < procLen; i++)
        {
            string guid = files[i];
            string filePath = AssetDatabase.GUIDToAssetPath(guid);

            T assetObj = AssetDatabase.LoadAssetAtPath<T>(filePath);
            //Debug.LogFormat(assetObj, "[ProcessAssets] asset:{0}", filePath);
            if (assetObj)
            {
                if (progress != null)
                {
                    progress(procTitle, filePath, assetObj, (float)i / procLen);
                }

                if (proc(assetObj, filePath))
                {
                    dirty = true;
                    EditorUtility.SetDirty(assetObj);
                }
            }
            else
            {
                Debug.LogErrorFormat("[ProcessAssets] asset failed to load:{0}", filePath);
            }
        }

        if (dirty)
        {
            AssetDatabase.SaveAssets();
        }

        EditorUtility.ClearProgressBar();
    }


    /// <summary>
    /// DefaultProcPrefabProgress
    /// </summary>
    /// <param name="title"></param>
    /// <param name="filePath"></param>
    /// <param name="procObject"></param>
    /// <param name="progress"></param>
    public static void DefaultProcPrefabProgress(string title, string filePath, GameObject procObject, float progress)
    {
        EditorUtility.DisplayProgressBar(title, string.Format("Processing prefab:{0}", filePath), progress);
    }

    /// <summary>    
    /// DefaultProcAssetProgress
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="filePath"></param>
    /// <param name="procObject"></param>
    /// <param name="progress"></param>
    public static void DefaultProcAssetProgress<T>(string title, string filePath, T procObject, float progress)
        where T : UnityEngine.Object
    {
        EditorUtility.DisplayProgressBar(title, string.Format("Processing asset:{0}", filePath), progress);
    }


}

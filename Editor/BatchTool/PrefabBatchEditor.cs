using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabBatchEditor
{
    public delegate void ListPrefabInstanceFun(GameObject prefab);
    public delegate bool ProcPrefabInstanceFun(GameObject prefabInstance);
	public static void ProcessAllPrefabs(ProcPrefabInstanceFun proc)
	{
		string[] files = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
		for (int i = 0; i < files.Length; i++)
		{
			string guid1 = files[i];
			string file = AssetDatabase.GUIDToAssetPath(guid1);

			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file);

			if (prefab)
			{
				Debug.LogFormat(prefab, "[ProcessAllPrefabs]Processing Prefab {0}:{1} {2}...", i, guid1, file);
				GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				if (proc(prefabInstance))
				{
					bool ok = false;
					PrefabUtility.SaveAsPrefabAsset(prefabInstance, file, out ok);
					if (!ok)
					{
						Debug.LogErrorFormat(prefab, "[ProcessAllPrefabs]Save to prefab failed:{0}", file);
					}
				}

				UnityEngine.Object.DestroyImmediate(prefabInstance);
			}
			else
			{
				Debug.LogErrorFormat("[ProcessAllPrefabs] LoadFailed:{0}", file);
			}
		}

		AssetDatabase.SaveAssets();
	}

    public static void ProcessAllPrefabsTemplate(ListPrefabInstanceFun proc)
    {
        string[] files = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        for (int i = 0; i < files.Length; i++)
        {
            string guid1 = files[i];
            string file = AssetDatabase.GUIDToAssetPath(guid1);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file);

            if (prefab)
            {
                Debug.LogFormat(prefab, "[ProcessAllPrefabsTemplate]Processing Prefab {0}:{1} {2}...", i, guid1, file);

                proc(prefab);
            }
            else
            {
                Debug.LogErrorFormat("[ProcessAllPrefabsTemplate] LoadFailed:{0}", file);
            }
        }
    }



    [MenuItem("资源工具/修正/移除Prefab中所有的丢失组件", false)]
	public static void RemoveMissingComponents()
	{
        ProcessAllPrefabs(RemoveMissingComponents);
	}

	static bool RemoveMissingComponents(GameObject prefabInstance)
	{
		if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefabInstance) > 0)
        {
            int cnt = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefabInstance);
            Debug.LogWarningFormat("Remove missing component:{0} Count:{1}", prefabInstance.name, cnt);
            return true;
        }

        return false;
	}

    [MenuItem("资源工具/修正/移除选中对象的丢失组件", false)]
    public static void RemoveMissingComponentsInstance()
    {
        if(Selection.objects.Length > 0)
        {
            foreach (var obj in Selection.objects)
            {
                GameObject go = (GameObject)obj;
                if(go != null)
                {
                    RemoveMissingComponents(go);
                }
            }
        }
    }

}

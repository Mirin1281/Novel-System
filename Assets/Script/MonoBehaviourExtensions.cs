using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

public static class MonoBehaviourExtensions
{
    /// <summary>
    /// シーン内のコンポーネントを検索します
    /// </summary>
    /// <typeparam name="T">コンポーネント</typeparam>
    /// <param name="objName">コンポーネントのついたオブジェクトの名前(コンポーネント名と同じなら省略可)</param>
    /// <param name="findInactive">非アクティブも検索する</param>
    /// <param name="callLog">呼ばれた際にログを出す</param>
    /// <returns></returns>
    public static T FindComponent<T>(
        this T self, string objName = null, bool findInactive = true, bool callLog = true)
        where T : MonoBehaviour
    {
#if UNITY_EDITOR
#else
        callLog = false;
#endif
        var componentName = typeof(T).Name;
        var findName = objName ?? componentName;
        var obj = GameObject.Find(findName);
        if (obj == null && findInactive)
        {
            obj = FindIncludInactive(findName);
        }
        if (obj == null)
        {
            Log(findName + " オブジェクトが見つかりませんでした", true);
            return null;
        }

        var component = obj.GetComponent<T>();
        if (component == null)
        {
            Log(componentName + " コンポーネントが見つかりませんでした", true);
            return null;
        }
        Log(componentName + "をFindしました", false);

        return component;


        void Log(string str, bool isWarning)
        {
            if (callLog == false) return;
            if (isWarning)
            {
                Debug.LogWarning("<color=red>" + str + "</color>");
            }
            else
            {
                Debug.Log("<color=lightblue>" + str + "</color>");
            }
        }


        /// <summary>
        /// 非アクティブのも含めてFindします
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        static GameObject FindIncludInactive(string targetName)
        {
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var gameObjectInHierarchy in gameObjects)
            {

#if UNITY_EDITOR
                //Hierarchy上のものでなければスルー
                if (!AssetDatabase.GetAssetOrScenePath(gameObjectInHierarchy).Contains(".unity"))
                {
                    continue;
                }
#endif
                if (gameObjectInHierarchy.name == targetName)
                {
                    return gameObjectInHierarchy;
                }
            }
            return null;
        }
    }
}

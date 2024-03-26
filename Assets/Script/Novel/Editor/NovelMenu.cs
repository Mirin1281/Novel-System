#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Novel
{
    public class NovelMenu
    {
        const string MenuName = "Tools/Novel/";
        const string FolderName = NameContainer.RESOURCES_PATH + "BaseObject/";

        [MenuItem(MenuName + "Create Flowchart")]
        static void CreateFlowchart()
        {
            string loadName = "Flowchart";
            var objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{FolderName}{loadName}.prefab");
            var obj = Object.Instantiate(objPrefab);
            obj.name = loadName;
        }

        [MenuItem(MenuName + "Create MessageBox")]
        static void CreateMessageBox()
        {
            string loadName = "MessageBox";
            var objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{FolderName}{loadName}.prefab");
            var obj = Object.Instantiate(objPrefab);
            obj.name = loadName;
        }

        [MenuItem(MenuName + "Create Portrait")]
        static void CreatePortrait()
        {
            string loadName = "Portrait";
            var objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{FolderName}{loadName}.prefab");
            var obj = Object.Instantiate(objPrefab);
            obj.name = loadName;
        }
    }
}
#endif
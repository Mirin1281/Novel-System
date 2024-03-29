using System.IO;
using UnityEditor;
using UnityEngine;

namespace Novel
{
    public static class FlowchartEditorUtility
    {
        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        public static string GetFileName(string path, string name)
        {
            int i = 1;
            var targetName = name;
            while (File.Exists($"{path}/{targetName}.asset"))
            {
                targetName = $"{name}_({i++})";
            }
            return $"{targetName}.asset";
        }
    }
}
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novel
{
    public static class FlowchartEditorUtility
    {

        /// <summary>
		/// 絶対パスから Assets/のパスに変換する
		/// </summary>
		public static string AbsoluteToAssetsPath(string path)
        {
            return path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }
        public static void DestroyScritableObject(ScriptableObject obj)
        {
            var path = GetExistFolderPath(obj);
            var deleteName = obj.name;
            Object.DestroyImmediate(obj, true);
            File.Delete($"{path}/{deleteName}.asset");
            File.Delete($"{path}/{deleteName}.asset.meta");
        }

        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(dataPath)) return null;
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        /// <summary>
        /// ファイル名を重複を考慮して付けます
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="name">名前(拡張子を除く)</param>
        /// <param name="extension">拡張子(.は除く)</param>
        /// <returns></returns>
        public static string GetFileName(string path, string name, string extension)
        {
            int i = 1;
            var targetName = name;
            while (File.Exists($"{path}/{targetName}.{extension}"))
            {
                targetName = $"{name}_({i++})";
            }
            return $"{targetName}.{extension}";
        }

        public static T[] GetAllScriptableObjects<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            return assetPaths.Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
        }
    }
}
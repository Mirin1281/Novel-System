using Novel.Command;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novel.Editor
{
    public static class FlowchartEditorUtility
    {
        /// <summary>
		/// ��΃p�X���� Assets/�̃p�X�ɕϊ�����
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
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(dataPath)) return null;
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        /// <summary>
        /// �t�@�C�������d�����l�����ĕt���܂�
        /// </summary>
        /// <param name="path">�p�X</param>
        /// <param name="name">���O(�g���q������)</param>
        /// <param name="extension">�g���q(.�͏���)</param>
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

        public static T[] GetAllScriptableObjects<T>(string folderName = null) where T : ScriptableObject
        {
            string[] guids = null;
            if (folderName == null)
            {
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            }
            else
            {
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folderName });
            }
            var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            return assetPaths.Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
        }

        public static CommandData CreateCommandData(string path, string baseName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var name = GetFileName(path, baseName, "asset");
            var cmdData = ScriptableObject.CreateInstance<CommandData>();
            AssetDatabase.CreateAsset(cmdData, Path.Combine(path, name));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return cmdData;
        }
    }
}
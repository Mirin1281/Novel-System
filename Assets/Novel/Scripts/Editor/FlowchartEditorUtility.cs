using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novel.Editor
{
    public static class FlowchartEditorUtility
    {
        /// <summary>
        /// 絶対パスから"Assets/"のパスに変換します
        /// </summary>
        public static string AbsoluteToAssetsPath(string path)
        {
            return path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }

        /// <summary>
        /// C#スクリプト名からパスを検索します
        /// </summary>
        public static bool TryGetScriptPath(string fileName, out string relativePath)
        {
            string path = null;
            var pathes = AssetDatabase.FindAssets(fileName + " t:Script")
                .Select(AssetDatabase.GUIDToAssetPath);
            var novelPathes = pathes.Where(s => s.Contains("Novel"));
            if (novelPathes.Count() == 0)
            {
                path = pathes.FirstOrDefault(str => string.Equals(Path.GetFileNameWithoutExtension(str),
                    fileName, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                path = novelPathes.FirstOrDefault(str => string.Equals(Path.GetFileNameWithoutExtension(str),
                    fileName, StringComparison.CurrentCultureIgnoreCase));
            }

            bool isExist = !string.IsNullOrEmpty(path);
            if (isExist)
            {
                relativePath = AbsoluteToAssetsPath(path);
            }
            else
            {
                relativePath = string.Empty;
            }
            return isExist;
        }

        /// <summary>
        /// 指定した名前を基に、フォルダ内で重複しない名前を生成します
        /// </summary>
        /// <param name="folderPath">対象となるフォルダのパス</param>
        /// <param name="baseName">名前(拡張子は除く)</param>
        /// <param name="extension">拡張子(.は除く)</param>
        /// <returns></returns>
        public static string GenerateAssetName(string folderPath, string baseName, string extension = null)
        {
            var paths = AssetDatabase.FindAssets(null, new string[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath);

            var existingNames = new HashSet<string>();
            foreach (var p in paths)
            {
                // サブフォルダは検索しなくていいので除外
                if (AbsoluteToAssetsPath(Path.GetDirectoryName(p)) != folderPath.TrimEnd('/')) continue;
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(p);
                foreach (var o in objs)
                {
                    existingNames.Add(o.name);
                }
            }

            string trimedName = baseName;
            string regex = @" \( ?\d+\)$";
            if (Regex.IsMatch(baseName, regex))
            {
                trimedName = Regex.Replace(baseName, regex, string.Empty);
            }

            string exStr = string.IsNullOrEmpty(extension) ? string.Empty : $".{extension}";

            // ベース名が重複しない場合、そのまま返す
            if (!existingNames.Contains(trimedName))
            {
                return $"{trimedName}{exStr}";
            }

            // 重複する場合、「(n)」を付けてユニークな名前を探す
            int suffix = 1;
            while (true)
            {
                string newName = $"{trimedName} ({suffix})";
                if (!existingNames.Contains(newName))
                {
                    return $"{newName}{exStr}";
                }
                suffix++;
            }
        }

        public static T[] GetAllScriptableObjects<T>(string folderName = null) where T : ScriptableObject
        {
            string[] guids = folderName == null
                ? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                : AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folderName });
            return guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
        }

        /// <summary>
        /// CommandDataを作成し、FlowchartDataの子に設定します
        /// </summary>
        /// <param name="parentData">親となるFlowchartData</param>
        /// <param name="baseName">名前</param>
        /// <returns></returns>
        public static CommandData CreateSubCommandData(FlowchartData parentData, string baseName)
        {
            var createdCmd = ScriptableObject.CreateInstance<CommandData>();
            string path = AssetDatabase.GetAssetPath(parentData);
            int lastIndex = path.LastIndexOf('/');
            string folderPath = path.Substring(0, lastIndex);
            createdCmd.name = GenerateAssetName(folderPath, baseName);
            AssetDatabase.AddObjectToAsset(createdCmd, parentData);
            Undo.RegisterCreatedObjectUndo(createdCmd, "Create Command");
            AssetDatabase.SaveAssets();
            return createdCmd;
        }

        public static CommandData DuplicateSubCommandData(FlowchartData parentData, CommandData cmdData)
        {
            var duplicatedCmd = Object.Instantiate(cmdData);
            string path = AssetDatabase.GetAssetPath(parentData);
            int lastIndex = path.LastIndexOf('/');
            string folderPath = path.Substring(0, lastIndex);
            duplicatedCmd.name = GenerateAssetName(folderPath, cmdData.name);
            AssetDatabase.AddObjectToAsset(duplicatedCmd, parentData);
            Undo.RegisterCreatedObjectUndo(duplicatedCmd, "Duplicate Command");
            AssetDatabase.SaveAssets();
            return duplicatedCmd;
        }

        /// <summary>
        /// 未使用のCommandDataを削除します
        /// </summary>
        public static void DestroyAllUnusedCommandData()
        {
            int removeCount = 0;
            foreach (var data in GetAllScriptableObjects<FlowchartData>())
            {
                removeCount += DestroyUnusedCommandData(data);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            if (removeCount > 0)
            {
                Debug.Log($"未使用のコマンドが合計{removeCount}個削除されました");
            }
            else
            {
                Debug.Log($"不要なデータの検索が完了しました。削除されたデータはありません");
            }


            // 指定したFlowchartDataで使われていないコマンドを削除
            static int DestroyUnusedCommandData(FlowchartData flowchartData)
            {
                int removeCount = 0;
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(flowchartData))
                    .Where(x => AssetDatabase.IsSubAsset(x));
                foreach (var sub in subAssets)
                {
                    CommandData cmdData = sub as CommandData;
                    if (flowchartData.IsUsed(cmdData) == false)
                    {
                        removeCount++;
                        Object.DestroyImmediate(cmdData, true);
                    }
                }

                if (removeCount > 0)
                {
                    Debug.Log($"{flowchartData.name} で未使用のコマンドが{removeCount}個削除されました");
                }
                return removeCount;
            }
        }
    }
}
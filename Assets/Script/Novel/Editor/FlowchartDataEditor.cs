using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Novel
{
    using Novel.Command;

    [CustomEditor(typeof(FlowchartData))]
    public class FlowchartDataEditor : Editor
    {
        readonly static string CommandDataPath = $"{NameContainer.RESOURCES_PATH}Commands";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(20);

            if (GUILayout.Button("フローチャートエディタを開く"))
            {
                EditorWindow.GetWindow<FlowchartEditorWindow>("Flowchart Editor");
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "【注意】\n" +
                "下のボタンから複製や削除をしてください。じゃないとバグります\n" +
                "\n" +
                $"また、{CommandDataPath} のデータは基本的に直接いじらないでください\n" +
                "\n" +
                "【もしもの対処】\n" +
                "もしも普通に複製してしまったら、そのまま削除すれば大丈夫です。普通に削除し\n" +
                "てしまった場合は「未使用のCommandDataを削除する」を押すとクリアされます\n" +
                "(ちょっと怖いのでGitなどでバックアップを取ることを推奨します)"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("複製する"))
            {
                var flowchartData = target as FlowchartData;
                var copiedFlowchartData = Instantiate(flowchartData);
                copiedFlowchartData.name = "CopiedFlowchartData";
                var folderPath = GetExistFolderPath(flowchartData);

                var dataName = GetFileName(folderPath, copiedFlowchartData.name);
                AssetDatabase.CreateAsset(copiedFlowchartData, Path.Combine(folderPath, dataName));
                AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate);
                var flowchart = copiedFlowchartData.Flowchart;

                var copiedCmdList = new List<CommandData>();
                foreach (var cmdData in flowchart.GetCommandDataList())
                {
                    var copiedCmdData = Instantiate(cmdData);
                    var cmd = copiedCmdData.GetCommandBase();
                    var cmdName = GetFileName(CommandDataPath, $"CommandData_{copiedFlowchartData.name}");
                    AssetDatabase.CreateAsset(copiedCmdData, Path.Combine(CommandDataPath, cmdName));
                    AssetDatabase.ImportAsset(CommandDataPath, ImportAssetOptions.ForceUpdate);
                    if (cmd != null)
                    {
                        cmd.SetFlowchart(flowchart);
                    }
                    copiedCmdList.Add(copiedCmdData);
                }
                flowchart.SetCommandDataList(copiedCmdList);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            if (GUILayout.Button("削除する"))
            {
                var flowchartData = target as FlowchartData;
                foreach (var cmdData in flowchartData.Flowchart.GetCommandDataList())
                {
                    DestroyScritableObject(cmdData);
                }
                DestroyScritableObject(flowchartData);
            }

            if (GUILayout.Button("未使用のCommandDataを削除する"))
            {
                var cmdDatas = GetAllScriptableObjects<CommandData>();
                var flowchartDatas = GetAllScriptableObjects<FlowchartData>();

                foreach (var cmdData in cmdDatas)
                {
                    if (IsUsed(cmdData, flowchartDatas) == false)
                    {
                        DestroyScritableObject(cmdData);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        void DestroyScritableObject(ScriptableObject obj)
        {
            var path = GetExistFolderPath(obj);
            var deleteCmdName = obj.name;
            DestroyImmediate(obj, true);
            File.Delete($"{path}/{deleteCmdName}.asset");
            File.Delete($"{path}/{deleteCmdName}.asset.meta");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        bool IsUsed(CommandData targetData, FlowchartData[] flowchartDatas)
        {
            foreach (var flowchartData in flowchartDatas)
            {
                if (flowchartData.IsUsed(targetData)) return true;
            }
            return false;
        }

        string GetFileName(string path, string name)
        {
            int i = 1;
            var targetName = name;
            while (File.Exists($"{path}/{targetName}.asset"))
            {
                targetName = $"{name}_({i++})";
            }
            return $"{targetName}.asset";
        }

        T[] GetAllScriptableObjects<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            return assetPaths.Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
        }
    }
}
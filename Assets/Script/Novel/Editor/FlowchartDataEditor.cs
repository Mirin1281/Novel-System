using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Novel
{
    using Novel.Command;

    [CustomEditor(typeof(FlowchartData))]
    public class FlowchartDataEditor : Editor
    {
        readonly static string CommandDataPath = $"{NameContainer.RESOURCES_PATH}Commands";
        readonly static string FlowchartDataPath = $"{NameContainer.RESOURCES_PATH}Flowchart";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField(
                "【重要】\n" +
                "普通にフローチャートを複製するとSerializeReference内部の関係で\n" +
                "参照が共有され結果としてコマンドデータのフィールドが連動してしまいます\n" +
                "それを防ぐために下のボタンから複製してください",
                EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("フローチャートを複製する"))
            {
                var flowchartData = target as FlowchartData;
                var copiedFlowchartData = Instantiate(flowchartData);
                copiedFlowchartData.name = "CopiedFlowchartData";
                var dataName = GetFileName(FlowchartDataPath, copiedFlowchartData.name);
                AssetDatabase.CreateAsset(copiedFlowchartData, Path.Combine(FlowchartDataPath, dataName));
                AssetDatabase.ImportAsset(FlowchartDataPath, ImportAssetOptions.ForceUpdate);
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

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "【重要】\n" +
                "普通に削除すると内部のScriptableObjectがCommandsフォルダ内に\n" +
                "残り続けるので、下のボタンから削除してください",
                EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("フローチャートを削除する"))
            {
                var flowchartData = target as FlowchartData;
                foreach (var cmdData in flowchartData.Flowchart.GetCommandDataList())
                {
                    var deleteCmdName = cmdData.name;
                    DestroyImmediate(cmdData, true);
                    File.Delete($"{CommandDataPath}/{deleteCmdName}.asset");
                    File.Delete($"{CommandDataPath}/{deleteCmdName}.asset.meta");
                }
                var deleteDataName = flowchartData.name;
                DestroyImmediate(flowchartData, true);
                File.Delete($"{FlowchartDataPath}/{deleteDataName}.asset");
                File.Delete($"{FlowchartDataPath}/{deleteDataName}.asset.meta");
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
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
    }
}
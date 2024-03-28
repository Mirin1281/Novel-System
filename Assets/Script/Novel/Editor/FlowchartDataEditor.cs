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

            if (GUILayout.Button("フローチャートエディタを開く"))
            {
                EditorWindow.GetWindow<FlowchartEditorWindow>("Flowchart Editor");
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "【注意】\n" +
                "普通にフローチャートを複製するとSerializeReference内部の関係で参照が共有され、\n" +
                "結果としてコマンドデータのフィールドが連動してしまうのでボタンから複製してください",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                "また、普通に削除するとCommandDataがCommandsフォルダ内に\n" +
                "残り続けてしまうので、下のボタンから削除してください",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "【対処】\n" +
                "もしctrl+Dで複製してしまったら、普通に削除すれば大丈夫です\n" +
                "普通に削除してしまった場合は放置するのを推奨します",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("複製する"))
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

            if (GUILayout.Button("削除する"))
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
            EditorGUILayout.EndHorizontal();
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
using UnityEngine;
using UnityEditor;

namespace Novel
{
using Novel.Command;
    using System.Collections.Generic;

    [CustomEditor(typeof(FlowchartExecutor))]
    public class FlowchartExecutorEditor : Editor
    {
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
                var flowchartExecutor = target as FlowchartExecutor;
                var copiedFlowchartExecutor = Instantiate(flowchartExecutor);
                copiedFlowchartExecutor.name = "Flowchart(Copied)";
                var flowchart = copiedFlowchartExecutor.Flowchart;

                var copiedCmdList = new List<CommandData>();
                foreach (var cmdData in flowchart.GetCommandDataList())
                {
                    var copiedCmdData = Instantiate(cmdData);
                    var cmd = copiedCmdData.GetCommandBase();
                    if(cmd != null)
                    {
                        cmd.SetFlowchart(flowchart);
                    }
                    copiedCmdList.Add(copiedCmdData);
                }
                flowchart.SetCommandDataList(copiedCmdList);
            }
        }
    }
}
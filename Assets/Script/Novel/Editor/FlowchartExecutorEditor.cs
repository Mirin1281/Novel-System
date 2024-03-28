using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Novel
{
    using Novel.Command;

    [CustomEditor(typeof(FlowchartExecutor))]
    public class FlowchartExecutorEditor : Editor
    {
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

            if (GUILayout.Button("複製する"))
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
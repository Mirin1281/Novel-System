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
                "下のボタンから複製をしてください。じゃないとバグります\n" +
                "\n" +
                "【もしもの対処】\n" +
                "もしも普通に複製してしまったら、そのまま削除すれば大丈夫です"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

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
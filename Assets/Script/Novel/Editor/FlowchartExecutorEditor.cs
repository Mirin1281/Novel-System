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

            if (GUILayout.Button("�t���[�`���[�g�G�f�B�^���J��"))
            {
                EditorWindow.GetWindow<FlowchartEditorWindow>("Flowchart Editor");
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "�y���Ӂz\n" +
                "���ʂɃt���[�`���[�g�𕡐������SerializeReference�����̊֌W�ŎQ�Ƃ����L����A\n" +
                "���ʂƂ��ăR�}���h�f�[�^�̃t�B�[���h���A�����Ă��܂��̂Ń{�^�����畡�����Ă�������",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("��������"))
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
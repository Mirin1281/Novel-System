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
                "�y�d�v�z\n" +
                "���ʂɃt���[�`���[�g�𕡐������SerializeReference�����̊֌W��\n" +
                "�Q�Ƃ����L���ꌋ�ʂƂ��ăR�}���h�f�[�^�̃t�B�[���h���A�����Ă��܂��܂�\n" +
                "�����h�����߂ɉ��̃{�^�����畡�����Ă�������",
                EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("�t���[�`���[�g�𕡐�����"))
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
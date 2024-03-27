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
                var flowchart = target as FlowchartExecutor;
                var copiedCmdList = new List<CommandData>();
                foreach(var cmd in flowchart.CommandDataList)
                {
                    var copiedCmd = Instantiate(cmd);
                    copiedCmdList.Add(copiedCmd);
                }
                var copiedFlowchart = Instantiate(flowchart);
                copiedFlowchart.name = "Flowchart(Copied)";
                copiedFlowchart.SetCommandList(copiedCmdList);
            }
        }
    }
}
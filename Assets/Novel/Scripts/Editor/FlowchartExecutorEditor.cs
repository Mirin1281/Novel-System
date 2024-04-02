using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

namespace Novel
{
    using Cysharp.Threading.Tasks;
    using Novel.Command;

    [CustomEditor(typeof(FlowchartExecutor))]
    public class FlowchartExecutorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("�t���[�`���[�g�G�f�B�^���J��"))
            {
                EditorWindow.GetWindow<FlowchartEditorWindow>("Flowchart Editor");
            }

            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "��������\n" +
                "���̃{�^�����畡�������Ă��������B����Ȃ��ƃo�O��܂�\n" +
                "�������ʂɕ������Ă��܂��Ă��A���̂܂܍폜����Α��v�ł�"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

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
                    if (cmd != null)
                    {
                        cmd.SetFlowchart(flowchart);
                    }
                    copiedCmdList.Add(copiedCmdData);
                }
                flowchart.SetCommandDataList(copiedCmdList);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "����CSV�̃G�N�X�|�[�g�ɂ���\n" +
                "�G�N�X�|�[�g����ƁA�V�[�����̑S�t���[�`���[�g�̃R�}���h�f�[�^���������܂�܂�\n" +
                "Excel��Google�X�v���b�h�V�[�g�œǂݍ��߂Ίm�F��ҏW�����邱�Ƃ��ł��܂�\n" +
                "CSVContent1, CSVContent2�Q�b�^�[���R�}���h���ɃI�[�o�[���C�h����\n" +
                "���Ƃŕ\��������e��ݒ�ł��܂� (�S���̓����ł���)\n" +
                "\n" +
                "����CSV�̃C���|�[�g�ɂ���\n" +
                "Excel�ł̃f�[�^�`���ɏ����Ă��܂�\n" +
                "CSV���̃V�[�����A�t���[�`���[�g���͕ς��Ȃ��ł�������\n" +
                "\n" +
                "�R�}���h����(�R�}���h�̃N���X������\"Command\"��������������)�ŁA���₷���Ƃ��ł��܂�\n" +
                "���e��CSVContent1, CSVContent2�Z�b�^�[���R�}���h���ɃI�[�o�[���C�h�����\n" +
                "�ǂݍ��ޓ��e��ݒ�ł��܂��Bgetter��setter�͑��ݕϊ��𐄏����܂�\n" +
                "\n" +
                "���łɂ���R�}���h��CSV��������@�\�͌���������Ă��܂���"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV�`���ŃG�N�X�|�[�g����"))
                {
                    var sceneName = SceneManager.GetActiveScene().name;
                    FlowchartCSVIO.ExportFlowchartCommandDataAsync(
                        sceneName, FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
                if (GUILayout.Button("CSV���C���|�[�g����"))
                {
                    var sceneName = SceneManager.GetActiveScene().name;
                    FlowchartCSVIO.ImportFlowchartCommandDataAsync(
                        sceneName, FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
            }
        }
    }
}
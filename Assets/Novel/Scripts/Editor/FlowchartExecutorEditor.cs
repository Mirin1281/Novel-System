using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Novel.Command;

namespace Novel.Editor
{
    [CustomEditor(typeof(FlowchartExecutor))]
    public class FlowchartExecutorEditor : UnityEditor.Editor
    {
        [SerializeField] bool isFold;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("�t���[�`���[�g�G�f�B�^���J��"))
            {
                EditorWindow.GetWindow<FlowchartEditorWindow>(
                    "Flowchart Editor", typeof(SceneView));
            }

            EditorGUILayout.Space(10);

            isFold = EditorGUILayout.Foldout(isFold, "More");
            if(isFold == false) return;

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
                copiedFlowchartExecutor.name = "Copied_"+ flowchartExecutor.name;
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
                "����CSV�̓��o�͂ɂ���\n" +
                "�G�N�X�|�[�g����ƁA�V�[�����̑SFlowchartExecutor�̃R�}���h�f�[�^���������܂�܂�\n" +
                "�C���|�[�g����ƁA�o�͂����`���Ńf�[�^����荞�ނ��Ƃ��ł��܂�\n" +
                "Excel�ł̃f�[�^�`���ɏ����Ă��܂��̂ŁA���̂܂܈����܂�\n" +
                "\n" +
                "�o�͂���f�[�^����͂���f�[�^��ݒ肵�����ꍇ�́ACommandBase����CSVContent1��CSVContent2�v���p�e�B��h���R�}���h���ɃI�[�o�[���C�h���Ă��������B�Q�b�^�[�ƃZ�b�^�[�͑��ݕϊ��ł���悤�ɂ��Ă�������\n" +
                "\n" +
                "�܂��A�c�̗�̃R�}���h�����R�ɑ��₷���Ƃ��ł��܂�\n" +
                "���łɂ���R�}���h��CSV��������@�\�͎������Ă��܂���B�Ƃ肠���������ɂ������ꍇ��\"Null\"�����Ă�������\n" +
                "\n" +
                "���ӓ_�Ƃ��āA�t�@�C�����͕ύX���Ă����܂��܂��񂪁ACSV����1�s�ڂ�3�s�ڂ̃f�[�^�͊�{�I�ɕς��Ȃ��ł�������"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV�`���ŃG�N�X�|�[�g����"))
                {
                    FlowchartCSVIO.ExportFlowchartCommandDataAsync(FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
                if (GUILayout.Button("CSV���C���|�[�g����"))
                {
                    FlowchartCSVIO.ImportFlowchartCommandDataAsync(FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
            }
        }
    }
}
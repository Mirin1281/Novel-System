using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Novel.Command;

namespace Novel.Editor
{
    [CustomEditor(typeof(FlowchartData))]
    public class FlowchartDataEditor : UnityEditor.Editor
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
                "���̃{�^�����畡����폜�����Ă��������B���ɕ����̓f�[�^�����L�����̂Œ���\n" +
                $"�܂��A{ConstContainer.COMMANDDATA_PATH} �̃f�[�^�͊�{�I�ɒ��ڂ�����Ȃ��ł�������\n" +
                "\n" +
                "�����������̑Ώ�\n" +
                "�������ʂɕ������Ă��܂������A���̂܂܍폜����Α��v�ł�\n" +
                "���ʂɍ폜���Ă��܂����ꍇ��A���h�D���������ȂǁA�R�}���h�f�[�^�����c�����\n" +
                "���Ƃ�����܂����A�u���g�p��CommandData���폜����v�������ƃN���A�ł��܂�\n" +
                "(������ƕ|���̂�Git�Ȃǌ��ɖ߂�����̗p�ӂ𐄏����܂�)"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("��������"))
                {
                    var flowchartData = target as FlowchartData;
                    var copiedFlowchartData = Instantiate(flowchartData);
                    copiedFlowchartData.name = "CopiedFlowchartData";
                    var folderPath = FlowchartEditorUtility.GetExistFolderPath(flowchartData);

                    var dataName = FlowchartEditorUtility.GetFileName(folderPath, copiedFlowchartData.name, "asset");
                    AssetDatabase.CreateAsset(copiedFlowchartData, Path.Combine(folderPath, dataName));
                    AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate);
                    var flowchart = copiedFlowchartData.Flowchart;

                    var copiedCmdList = new List<CommandData>();
                    foreach (var cmdData in flowchart.GetReadOnlyCommandDataList())
                    {
                        var copiedCmdData = Instantiate(cmdData);
                        var cmd = copiedCmdData.GetCommandBase();
                        var path = FlowchartEditorUtility.GetExistFolderPath(cmdData);
                        var cmdName = FlowchartEditorUtility.GetFileName(path, $"CommandData_{copiedFlowchartData.name}", "asset");
                        AssetDatabase.CreateAsset(copiedCmdData, Path.Combine(path, cmdName));
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        if (cmd != null)
                        {
                            cmd.SetFlowchart(flowchart);
                        }
                        copiedCmdList.Add(copiedCmdData);
                    }
                    flowchart.SetCommandDataList(copiedCmdList);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                if (GUILayout.Button("�폜����"))
                {
                    var flowchartData = target as FlowchartData;
                    foreach (var cmdData in flowchartData.Flowchart.GetCommandDataList())
                    {
                        FlowchartEditorUtility.DestroyScritableObject(cmdData);
                    }
                    FlowchartEditorUtility.DestroyScritableObject(flowchartData);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                if (GUILayout.Button("���g�p��CommandData���폜����"))
                {
                    var cmdDatas = FlowchartEditorUtility.GetAllScriptableObjects<CommandData>();
                    var flowchartDatas = FlowchartEditorUtility.GetAllScriptableObjects<FlowchartData>();

                    foreach (var cmdData in cmdDatas)
                    {
                        if (IsUsed(cmdData, flowchartDatas) == false)
                        {
                            FlowchartEditorUtility.DestroyScritableObject(cmdData);
                        }
                    }
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "����CSV�̓��o�͂ɂ���\n" +
                "�G�N�X�|�[�g����ƁA�v���W�F�N�g�t�H���_���̑SFlowchartData�̃R�}���h�f�[�^���������܂�܂�\n" +
                "�C���|�[�g����ƁA�o�͂����`���Ńf�[�^����荞�ނ��Ƃ��ł��܂�\n" +
                "Excel�ł̃f�[�^�`���ɏ����Ă��܂��̂ŁA���̂܂܈����܂�\n" +
                "\n" +
                "�o�͂���f�[�^����͂���f�[�^��ݒ肵�����ꍇ�́ACommandBase����CSVContent1��CSVContent2�v���p�e�B��h���R�}���h���ɃI�[�o�[���C�h���Ă��������B�Q�b�^�[�ƃZ�b�^�[�͑��ݕϊ��ł���悤�ɂ��Ă�������\n" +
                "\n" +
                "�܂��A�c�̗�̃R�}���h�����R�ɑ��₷���Ƃ��ł��܂�\n" +
                "���łɂ���R�}���h��CSV��������@�\�͎������Ă��܂���B�Ƃ肠���������ɂ������ꍇ��\"Null\"�����Ă�������\n" +
                "\n" +
                "���ӓ_�Ƃ��āA�t�@�C�����͕ύX���Ă����܂��܂��񂪁ACSV����1�s�ڂ�2�s�ڂ̃f�[�^�͊�{�I�ɕς��Ȃ��ł�������"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV�`���ŃG�N�X�|�[�g����"))
                {
                    FlowchartCSVIO.ExportFlowchartCommandDataAsync(FlowchartCSVIO.FlowchartType.Data).Forget();
                }
                if (GUILayout.Button("CSV���C���|�[�g����"))
                {
                    FlowchartCSVIO.ImportFlowchartCommandDataAsync(FlowchartCSVIO.FlowchartType.Data).Forget();
                }
            }
        }        

        bool IsUsed(CommandData targetData, FlowchartData[] flowchartDatas)
        {
            foreach (var flowchartData in flowchartDatas)
            {
                if (flowchartData.IsUsed(targetData)) return true;
            }
            return false;
        }
    }
}
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

            EditorGUILayout.LabelField(
                "�܂��A���ʂɍ폜�����CommandData��Commands�t�H���_����\n" +
                "�c�葱���Ă��܂��̂ŁA���̃{�^������폜���Ă�������",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "�y�Ώ��z\n" +
                "����ctrl+D�ŕ������Ă��܂�����A���ʂɍ폜����Α��v�ł�\n" +
                "���ʂɍ폜���Ă��܂����ꍇ�͕��u����̂𐄏����܂�",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("��������"))
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

            if (GUILayout.Button("�폜����"))
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
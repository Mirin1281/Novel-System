using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Novel
{
    using Novel.Command;

    [CustomEditor(typeof(FlowchartData))]
    public class FlowchartDataEditor : Editor
    {
        readonly static string CommandDataPath = $"{NameContainer.RESOURCES_PATH}Commands";

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
                "���̃{�^�����畡����폜�����Ă��������B����Ȃ��ƃo�O��܂�\n" +
                "\n" +
                $"�܂��A{CommandDataPath} �̃f�[�^�͊�{�I�ɒ��ڂ�����Ȃ��ł�������\n" +
                "\n" +
                "�y�������̑Ώ��z\n" +
                "���������ʂɕ������Ă��܂�����A���̂܂܍폜����Α��v�ł��B���ʂɍ폜��\n" +
                "�Ă��܂����ꍇ�́u���g�p��CommandData���폜����v�������ƃN���A����܂�\n" +
                "(������ƕ|���̂�Git�ȂǂŃo�b�N�A�b�v����邱�Ƃ𐄏����܂�)"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("��������"))
            {
                var flowchartData = target as FlowchartData;
                var copiedFlowchartData = Instantiate(flowchartData);
                copiedFlowchartData.name = "CopiedFlowchartData";
                var folderPath = GetExistFolderPath(flowchartData);

                var dataName = GetFileName(folderPath, copiedFlowchartData.name);
                AssetDatabase.CreateAsset(copiedFlowchartData, Path.Combine(folderPath, dataName));
                AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate);
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
                    DestroyScritableObject(cmdData);
                }
                DestroyScritableObject(flowchartData);
            }

            if (GUILayout.Button("���g�p��CommandData���폜����"))
            {
                var cmdDatas = GetAllScriptableObjects<CommandData>();
                var flowchartDatas = GetAllScriptableObjects<FlowchartData>();

                foreach (var cmdData in cmdDatas)
                {
                    if (IsUsed(cmdData, flowchartDatas) == false)
                    {
                        DestroyScritableObject(cmdData);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        void DestroyScritableObject(ScriptableObject obj)
        {
            var path = GetExistFolderPath(obj);
            var deleteCmdName = obj.name;
            DestroyImmediate(obj, true);
            File.Delete($"{path}/{deleteCmdName}.asset");
            File.Delete($"{path}/{deleteCmdName}.asset.meta");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        bool IsUsed(CommandData targetData, FlowchartData[] flowchartDatas)
        {
            foreach (var flowchartData in flowchartDatas)
            {
                if (flowchartData.IsUsed(targetData)) return true;
            }
            return false;
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

        T[] GetAllScriptableObjects<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            return assetPaths.Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
        }
    }
}
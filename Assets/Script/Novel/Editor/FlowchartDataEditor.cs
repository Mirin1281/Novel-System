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
                "下のボタンから複製や削除をしてください。特に複製はデータが共有されるので注意\n" +
                $"また、{NameContainer.COMMANDDATA_PATH} のデータは基本的に直接いじらないでください\n" +
                "\n" +
                "【もしもの対処】\n" +
                "もしも普通に複製してしまったら、そのまま削除すれば大丈夫です\n" +
                "普通に削除してしまった場合やアンドゥをした時など、コマンドデータが取り残される\n" +
                "ことがありますが、「未使用のCommandDataを削除する」を押すとクリアできます\n" +
                "(ちょっと怖いのでGitなど元に戻せる環境の用意を推奨します)"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("複製する"))
            {
                var flowchartData = target as FlowchartData;
                var copiedFlowchartData = Instantiate(flowchartData);
                copiedFlowchartData.name = "CopiedFlowchartData";
                var folderPath = FlowchartEditorUtility.GetExistFolderPath(flowchartData);

                var dataName = FlowchartEditorUtility.GetFileName(folderPath, copiedFlowchartData.name);
                AssetDatabase.CreateAsset(copiedFlowchartData, Path.Combine(folderPath, dataName));
                AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate);
                var flowchart = copiedFlowchartData.Flowchart;

                var copiedCmdList = new List<CommandData>();
                foreach (var cmdData in flowchart.GetReadOnlyCommandDataList())
                {
                    var copiedCmdData = Instantiate(cmdData);
                    var cmd = copiedCmdData.GetCommandBase();
                    var path = FlowchartEditorUtility.GetExistFolderPath(cmdData);
                    var cmdName = FlowchartEditorUtility.GetFileName(path, $"CommandData_{copiedFlowchartData.name}");
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

            if (GUILayout.Button("削除する"))
            {
                var flowchartData = target as FlowchartData;
                foreach (var cmdData in flowchartData.Flowchart.GetCommandDataList())
                {
                    DestroyScritableObject(cmdData);
                }
                DestroyScritableObject(flowchartData);
            }

            if (GUILayout.Button("未使用のCommandDataを削除する"))
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

        void DestroyScritableObject(ScriptableObject obj)
        {
            var path = FlowchartEditorUtility.GetExistFolderPath(obj);
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

        T[] GetAllScriptableObjects<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            return assetPaths.Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
        }
    }
}
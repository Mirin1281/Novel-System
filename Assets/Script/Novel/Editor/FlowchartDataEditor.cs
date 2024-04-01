using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Novel.Command;

namespace Novel
{
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
                "◆◆注意\n" +
                "下のボタンから複製や削除をしてください。特に複製はデータが共有されるので注意\n" +
                $"また、{NameContainer.COMMANDDATA_PATH} のデータは基本的に直接いじらないでください\n" +
                "\n" +
                "◆◆もしもの対処\n" +
                "もし普通に複製してしまったも、そのまま削除すれば大丈夫です\n" +
                "普通に削除してしまった場合やアンドゥをした時など、コマンドデータが取り残される\n" +
                "ことがありますが、「未使用のCommandDataを削除する」を押すとクリアできます\n" +
                "(ちょっと怖いのでGitなど元に戻せる環境の用意を推奨します)"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("複製する"))
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

                if (GUILayout.Button("削除する"))
                {
                    var flowchartData = target as FlowchartData;
                    foreach (var cmdData in flowchartData.Flowchart.GetCommandDataList())
                    {
                        FlowchartEditorUtility.DestroyScritableObject(cmdData);
                    }
                    FlowchartEditorUtility.DestroyScritableObject(flowchartData);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                if (GUILayout.Button("未使用のCommandDataを削除する"))
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
                "◆◆CSVのエクスポートについて\n" +
                "エクスポートすると、シーン内の全フローチャートのコマンドデータが書き込まれます\n" +
                "ExcelやGoogleスプレッドシートで読み込めば確認や編集をすることができます\n" +
                "CSVContent1, CSVContent2ゲッターをコマンド内にオーバーライドする\n" +
                "ことで表示する内容を設定できます (全部はムリですが)\n" +
                "\n" +
                "◆◆CSVのインポートについて\n" +
                "Excelでのデータ形式に準じています\n" +
                "CSV内のシーン名、フローチャート名は変えないでください\n" +
                "\n" +
                "コマンド名は(コマンドのクラス名から\"Command\"を除いた文字列)で、増やすこともできます\n" +
                "内容はCSVContent1, CSVContent2セッターをコマンド内にオーバーライドすると\n" +
                "読み込む内容を設定できます。getterとsetterは相互変換を推奨します\n" +
                "\n" +
                "すでにあるコマンドをCSVから消す機能は現状実装していません"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV形式でエクスポートする"))
                {
                    FlowchartCSVIO.ExportFlowchartCommandDataAsync(
                        "ScriptableObjects", FlowchartCSVIO.FlowchartType.Data).Forget();
                }
                if (GUILayout.Button("CSVをインポートする"))
                {
                    FlowchartCSVIO.ImportFlowchartCommandDataAsync(
                        "ScriptableObjects", FlowchartCSVIO.FlowchartType.Data).Forget();
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
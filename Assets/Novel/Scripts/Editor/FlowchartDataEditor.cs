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

            if (GUILayout.Button("フローチャートエディタを開く"))
            {
                EditorWindow.GetWindow<FlowchartEditorWindow>(
                    "Flowchart Editor", typeof(SceneView));
            }

            EditorGUILayout.Space(10);

            isFold = EditorGUILayout.Foldout(isFold, "More");
            if(isFold == false) return;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "◆◆注意\n" +
                "下のボタンから複製や削除をしてください。特に複製はデータが共有されるので注意\n" +
                $"また、{ConstContainer.COMMANDDATA_PATH} のデータは基本的に直接いじらないでください\n" +
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
                "◆◆CSVの入出力について\n" +
                "エクスポートすると、プロジェクトフォルダ内の全FlowchartDataのコマンドデータが書き込まれます\n" +
                "インポートすると、出力した形式でデータを取り込むことができます\n" +
                "Excelでのデータ形式に準じていますので、そのまま扱えます\n" +
                "\n" +
                "出力するデータや入力するデータを設定したい場合は、CommandBase内のCSVContent1やCSVContent2プロパティを派生コマンド内にオーバーライドしてください。ゲッターとセッターは相互変換できるようにしてください\n" +
                "\n" +
                "また、縦の列のコマンドを自由に増やすこともできます\n" +
                "すでにあるコマンドをCSVから消す機能は実装していません。とりあえず無効にしたい場合は\"Null\"を入れてください\n" +
                "\n" +
                "注意点として、ファイル名は変更してもかまいませんが、CSV内の1行目と2行目のデータは基本的に変えないでください"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV形式でエクスポートする"))
                {
                    FlowchartCSVIO.ExportFlowchartCommandDataAsync(FlowchartCSVIO.FlowchartType.Data).Forget();
                }
                if (GUILayout.Button("CSVをインポートする"))
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
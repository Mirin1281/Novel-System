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
            if (GUILayout.Button("フローチャートエディタを開く"))
            {
                EditorWindow.GetWindow<FlowchartEditorWindow>("Flowchart Editor");
            }

            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "◆◆注意\n" +
                "下のボタンから複製をしてください。じゃないとバグります\n" +
                "もし普通に複製してしまっても、そのまま削除すれば大丈夫です"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("複製する"))
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
                    var sceneName = SceneManager.GetActiveScene().name;
                    FlowchartCSVIO.ExportFlowchartCommandDataAsync(
                        sceneName, FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
                if (GUILayout.Button("CSVをインポートする"))
                {
                    var sceneName = SceneManager.GetActiveScene().name;
                    FlowchartCSVIO.ImportFlowchartCommandDataAsync(
                        sceneName, FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
            }
        }
    }
}
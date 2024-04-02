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
                "◆◆CSVの入出力について\n" +
                "エクスポートすると、シーン内の全FlowchartExecutorのコマンドデータが書き込まれます\n" +
                "インポートすると、出力した形式でデータを取り込むことができます\n" +
                "Excelでのデータ形式に準じていますので、そのまま扱えます\n" +
                "\n" +
                "出力するデータや入力するデータを設定したい場合は、CommandBase内のCSVContent1やCSVContent2プロパティを派生コマンド内にオーバーライドしてください。ゲッターとセッターは相互変換を推奨します\n" +
                "\n" +
                "また、縦の列のコマンドを自由に増やすこともできます。コマンドを自作する場合、クラス名を\"〜Command\"としてください\n" +
                "すでにあるコマンドをCSVから消す機能は実装していません。とりあえず無効にしたい場合は\"<Null>\"または単に\"Null\"を入れてください\n" +
                "\n" +
                "注意点として、ファイル名は変更してもかまいませんが、CSV内の1行目と2行目のデータは基本的に変えないでください"
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
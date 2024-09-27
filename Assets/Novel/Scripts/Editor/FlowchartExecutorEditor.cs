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
                "下のボタンから複製をしてください。じゃないとバグります\n" +
                "もし普通に複製してしまっても、そのまま削除すれば大丈夫です"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("複製する"))
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
                "◆◆CSVの入出力について\n" +
                "エクスポートすると、シーン内の全FlowchartExecutorのコマンドデータが書き込まれます\n" +
                "インポートすると、出力した形式でデータを取り込むことができます\n" +
                "Excelでのデータ形式に準じていますので、そのまま扱えます\n" +
                "\n" +
                "出力するデータや入力するデータを設定したい場合は、CommandBase内のCSVContent1やCSVContent2プロパティを派生コマンド内にオーバーライドしてください。ゲッターとセッターは相互変換できるようにしてください\n" +
                "\n" +
                "また、縦の列のコマンドを自由に増やすこともできます\n" +
                "すでにあるコマンドをCSVから消す機能は実装していません。とりあえず無効にしたい場合は\"Null\"を入れてください\n" +
                "\n" +
                "注意点として、ファイル名は変更してもかまいませんが、CSV内の1行目と3行目のデータは基本的に変えないでください"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV形式でエクスポートする"))
                {
                    FlowchartCSVIO.ExportFlowchartCommandDataAsync(FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
                if (GUILayout.Button("CSVをインポートする"))
                {
                    FlowchartCSVIO.ImportFlowchartCommandDataAsync(FlowchartCSVIO.FlowchartType.Executor).Forget();
                }
            }
        }
    }
}
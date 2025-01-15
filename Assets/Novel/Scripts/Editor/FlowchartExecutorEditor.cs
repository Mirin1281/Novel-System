using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Novel.Command;
using System.Text.RegularExpressions;

namespace Novel.Editor
{
    [CustomEditor(typeof(FlowchartExecutor))]
    public class FlowchartExecutorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "◆注意\n" +
                "複製する場合は下のボタンから複製をしてください。もしctrl+Dで複製してしまっても、そのまま削除すれば大丈夫です"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Duplicate"))
            {
                DupulicateFrom(target as FlowchartExecutor);
            }
        }

        FlowchartExecutor DupulicateFrom(FlowchartExecutor baseExecutor)
        {
            var copiedExecutor = Instantiate(baseExecutor);
            copiedExecutor.name = GenerateHierarchyName(baseExecutor.name);
            var flowchart = copiedExecutor.Flowchart;

            Undo.RegisterCreatedObjectUndo(copiedExecutor.gameObject, "Duplicate Flowchart");

            var copiedCmdList = new List<CommandData>();
            foreach (var cmdData in flowchart.GetCommandDataList())
            {
                var copiedCmdData = Instantiate(cmdData);
                var cmd = copiedCmdData.GetCommandBase();
                cmd?.SetFlowchart(flowchart);
                copiedCmdList.Add(copiedCmdData);
            }
            flowchart.SetCommandDataList(copiedCmdList);

            // ヒエラルキーの位置を調整
            copiedExecutor.transform.SetParent(baseExecutor.transform.parent);
            int siblingIndex = baseExecutor.transform.GetSiblingIndex();
            copiedExecutor.transform.SetSiblingIndex(siblingIndex + 1);

            Selection.activeGameObject = copiedExecutor.gameObject;
            return copiedExecutor;
        }

        /// <summary>
        /// 指定した名前を基に、ヒエラルキー内で重複しない名前を生成する
        /// </summary>
        static string GenerateHierarchyName(string baseName)
        {
            // ヒエラルキー内のすべてのゲームオブジェクトを取得
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // ヒエラルキーに存在する名前をリスト化
            var existingNames = new HashSet<string>();
            foreach (var obj in allObjects)
            {
                existingNames.Add(obj.name);
            }

            string trimedName = baseName;
            string regex = @" \( ?\d+\)$";
            if (Regex.IsMatch(baseName, regex))
            {
                trimedName = Regex.Replace(baseName, regex, string.Empty);
            }

            // ベース名が重複しない場合、そのまま返す
            if (!existingNames.Contains(trimedName))
            {
                return trimedName;
            }

            // 重複する場合、「(n)」を付けてユニークな名前を探す
            int suffix = 1;
            while (true)
            {
                string newName = $"{trimedName} ({suffix})";
                if (!existingNames.Contains(newName))
                {
                    return newName;
                }
                suffix++;
            }
        }
    }
}
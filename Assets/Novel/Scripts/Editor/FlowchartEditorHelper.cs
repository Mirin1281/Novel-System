using Novel.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Novel.Editor
{
    [Serializable]
    public class FlowchartEditorHelper
    {
        public FlowchartEditorHelper(EditorWindow window)
        {
            this.window = window;
        }

        [SerializeField] EditorWindow window;

        enum ActiveType
        {
            None,
            Executor,
            Data,
        }

        /// <summary>
        /// ExecutorとDataのどちらのFlowchartが選択されているか
        /// </summary>
        [SerializeField] ActiveType activeType;

        [SerializeField] Flowchart activeFlow;
        GameObject activeExecutorObj;
        [SerializeField] FlowchartData activeFlowchartData;

        /// <summary>
        /// 表示用のコマンドリスト
        /// </summary>
        [SerializeField] List<CommandData> commandList;

        List<int> selectedIndices;
        int? LastSelectedIndex
        {
            get
            {
                if (selectedIndices == null || selectedIndices.Count == 0) return null;
                return selectedIndices.Last();
            }
        }
        CommandData LastSelectedCommand
        {
            get
            {
                if (selectedIndices == null || selectedIndices.Count == 0
                 || commandList == null || commandList.Count == 0) return null;
                return commandList[Mathf.Clamp(selectedIndices.Last(), 0, commandList.Count - 1)];
            }
        }

        /// <summary>
        /// コマンド操作のUndoをする際、直前に変化したコマンドのインデックスが知りたい
        /// </summary>
        List<int> updatedIndices;

        List<CommandData> copiedCommandList;

        [SerializeField] ReorderableList reorderableList;

        /// <summary>
        /// FlowchartExecutorの選択状態を、エディタを閉じた後も記録するための名前
        /// </summary>
        [SerializeField] string selectedName;

        Vector2 listScrollPos;
        Vector2 commandScrollPos;

        int reorderFromIndex;
        int reorderToIndex;

        /// <summary>
        /// ウィンドウの分割する割合
        /// </summary>
        static readonly float WindowSplitRatio = 0.5f;

        /// <summary>
        /// コマンドを選択した際の色
        /// </summary>
        static readonly Color CommandSelectedColor = new Color(0.4f, 0.9f, 0.95f, 0.9f);


        static void Log(string message, bool isWarning = false)
        {
            string text = $"<color=lightblue>{message}</color>";
            if (isWarning)
            {
                Debug.LogWarning(text);
            }
            else
            {
                Debug.Log(text);
            }
        }

        void SetCommandToFlowchart(IEnumerable<CommandData> list)
        {
            if (activeFlow == null) return;
            activeFlow.SetCommandDataList(list);
            SetObjectDirty();
        }

        void SetObjectDirty()
        {
            if (activeType == ActiveType.Executor)
            {
                EditorUtility.SetDirty(activeExecutorObj);
            }
            else if (activeType == ActiveType.Data)
            {
                EditorUtility.SetDirty(activeFlowchartData);
            }
        }

        public void SetEvents(bool enable)
        {
            if (enable)
            {
                EditorApplication.hierarchyChanged += OnHierarchyOrProjectChanged;
                EditorApplication.projectChanged += OnHierarchyOrProjectChanged;
                EditorSceneManager.sceneOpened += OnSceneChangedInEditor;
                EditorApplication.playModeStateChanged += OnChangePlayMode;
                Undo.undoRedoEvent += OnUndoRedo;
            }
            else
            {
                EditorApplication.hierarchyChanged -= OnHierarchyOrProjectChanged;
                EditorApplication.projectChanged -= OnHierarchyOrProjectChanged;
                EditorSceneManager.sceneOpened -= OnSceneChangedInEditor;
                EditorApplication.playModeStateChanged -= OnChangePlayMode;
                Undo.undoRedoEvent -= OnUndoRedo;
            }


            void OnHierarchyOrProjectChanged()
            {
                if (EditorApplication.isPlaying) return;
                UpdateFlowchartObjectAndWindow();
                // 削除時 
                if (Selection.activeObject == null)
                {
                    if (activeType == ActiveType.Executor && activeExecutorObj == null
                    || activeType == ActiveType.Data && activeFlowchartData == null)
                    {
                        activeType = ActiveType.None;
                        activeFlow = null;
                        window.Repaint();
                    }
                }
            }

            void OnSceneChangedInEditor(Scene _, OpenSceneMode __)
            {
                // シーン変更時かつExecutor選択時、エディタをリセット
                if (activeType == ActiveType.Executor)
                {
                    activeType = ActiveType.None;
                    activeExecutorObj = null;
                    activeFlow = null;

                    reorderableList = null;
                    selectedIndices = null;
                    updatedIndices = null;
                    copiedCommandList?.Clear();

                    selectedName = string.Empty;
                    listScrollPos = Vector2.zero;
                    commandScrollPos = Vector2.zero;
                }
            }

            void OnChangePlayMode(PlayModeStateChange state)
            {
                // モードが変化した際に処理をする
                if (state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode)
                {
                    UpdateActiveFlowchart();
                }
            }

            void OnUndoRedo(in UndoRedoInfo info)
            {
                OperateType operate = GetUndoRedoType(info);
                if (operate == OperateType.None) return;

                AssetDatabase.SaveAssets();

                if (operate is OperateType.Remove && activeType == ActiveType.Data && info.isRedo == false)
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(activeFlowchartData))
                        .Where(x => AssetDatabase.IsSubAsset(x));

                    // Unity特有の偽nullに対処するため、サブアセットを直接更新する
                    var list = activeFlowchartData.Flowchart.GetCommandDataList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (var sub in subAssets)
                        {
                            // Insane Code
                            CommandData convertedCmd = sub as CommandData;
                            if (list[i] == convertedCmd)
                            {
                                list[i] = convertedCmd;
                            }
                        }
                    }
                    commandList = list;
                }
                else
                {
                    SetCommandToFlowchart(commandList);
                }
                reorderableList = CreateReorderableList();

                if (operate is OperateType.Add or OperateType.Paste or OperateType.Duplicate && info.isRedo)
                {
                    int updatedIndex = updatedIndices.FirstOrDefault();
                    SelectCommand(updatedIndex);
                }
                else if (operate is OperateType.Add or OperateType.Paste or OperateType.Duplicate
                    || operate is OperateType.Remove && info.isRedo)
                {
                    int updatedIndex = updatedIndices.FirstOrDefault();
                    int lastIndex = LastSelectedIndex.GetValueOrDefault();
                    if (updatedIndex <= lastIndex)
                    {
                        SelectCommand(lastIndex - 1);
                    }
                    else
                    {
                        SelectCommand(lastIndex);
                    }
                }
                else if (operate == OperateType.Remove)
                {
                    // Undo前に選択していたコマンドを選択状態にする
                    SelectCommand(null);
                    foreach (var i in updatedIndices)
                    {
                        SelectCommand(i, append: true);
                    }
                }
                else if (operate == OperateType.Reorder)
                {
                    int from = reorderFromIndex;
                    int to = reorderToIndex;
                    if (info.isRedo) // Redo時は逆にする
                        (from, to) = (to, from);

                    // 例えば「0 => 2」へ移動したら、「2 => 0」までスワップして戻す
                    if (from < to)
                    {
                        for (int i = to; i > from; i--)
                        {
                            (commandList[i], commandList[i - 1]) = (commandList[i - 1], commandList[i]);
                        }
                    }
                    else
                    {
                        for (int i = to; i < from; i++)
                        {
                            (commandList[i], commandList[i + 1]) = (commandList[i + 1], commandList[i]);
                        }
                    }
                    SelectCommand(from);
                    SetCommandToFlowchart(commandList);
                }

                window.Repaint();
            }
        }

        public void Init()
        {
            // エディタの起動時に前回選択していたフローチャートを復元
            if (activeType == ActiveType.Executor)
            {
                var executor = GameObject.FindObjectsByType<FlowchartExecutor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(e => e.name == selectedName);
                if (executor != null)
                {
                    activeExecutorObj = executor.gameObject;
                }
            }

            SetEvents(false);
            SetEvents(true);
            commandList ??= new();
            selectedIndices ??= new();
            updatedIndices ??= new();
            UpdateFlowchartObjectAndWindow();
            UpdateActiveFlowchart();
        }

        void UpdateWindow()
        {
            if (activeFlow == null) return;
            AssetDatabase.SaveAssets();
            reorderableList = CreateReorderableList();
            selectedIndices.ForEach(s => reorderableList.Select(s, true));
            window.Repaint();
        }

        public void UpdateFlowchartObjectAndWindow()
        {
            bool isChanged = false;

            GameObject activeObj = Selection.activeGameObject;
            if (activeObj != null && activeObj.TryGetComponent<FlowchartExecutor>(out var executor))
            {
                // Executorがヒット 
                isChanged = executor.Flowchart.EqualsCommands(activeFlow) == false;
                // 本来は下のようにしたいが、コンパイル時だと同じフローチャートでも不一致判定が出るためコマンドリストを照合している
                // isChanged = activeFlow != executor.Flowchart;
                activeType = ActiveType.Executor;
                activeExecutorObj = activeObj;
                activeFlow = executor.Flowchart;
            }
            else if (Selection.activeObject != null)
            {
                var data = Selection.GetFiltered<FlowchartData>(SelectionMode.Assets).FirstOrDefault();
                if (data != null)
                {
                    // Dataがヒット
                    isChanged = data.Flowchart.EqualsCommands(activeFlow) == false;
                    activeType = ActiveType.Data;
                    activeFlowchartData = data;
                    activeFlow = data.Flowchart;
                }
            }

            if (isChanged)
            {
                commandList = activeFlow.GetCommandDataList();
                selectedIndices = new();
                updatedIndices = new();
                listScrollPos = Vector2.zero;
                commandScrollPos = Vector2.zero;
                selectedName = Selection.activeObject.name;
                UpdateWindow();
            }
        }

        void UpdateActiveFlowchart()
        {
            if (activeType == ActiveType.Executor && activeExecutorObj != null)
            {
                activeFlow = activeExecutorObj.GetComponent<FlowchartExecutor>().Flowchart;
            }
            else if (activeType == ActiveType.Data && activeFlowchartData != null)
            {
                activeFlow = activeFlowchartData.Flowchart;
            }

            if (activeFlow != null)
                commandList = activeFlow.GetCommandDataList();
            UpdateWindow();
        }

        enum OperateType
        {
            None,
            Add,
            Remove,
            Paste,
            Duplicate,
            Reorder,
        }

        void SetRecordUndoName(OperateType operate)
        {
            string recordName = operate switch
            {
                OperateType.Add => "Add Command",
                OperateType.Remove => "Remove Command",
                OperateType.Paste => "Paste Command",
                OperateType.Duplicate => "Duplicate Command",
                OperateType.Reorder => "Reorder Command",
                _ => throw new Exception($"{nameof(OperateType)} is None")
            };
            Undo.SetCurrentGroupName(recordName);
        }

        OperateType GetUndoRedoType(UndoRedoInfo info)
        {
            return info.undoName switch
            {
                "Add Command" => OperateType.Add,
                "Remove Command" => OperateType.Remove,
                "Paste Command" => OperateType.Paste,
                "Duplicate Command" => OperateType.Duplicate,
                "Reorder Command" => OperateType.Reorder,
                _ => OperateType.None
            };
        }

        #region OnGUI

        public void OnGUI()
        {
            // selectedIndicesのデバッグ用 
            /*if (selectedIndices != null)
            {
                string s = string.Empty;
                foreach (var i in selectedIndices)
                {
                    s += $"{i} ";
                }
                if (string.IsNullOrEmpty(s) == false)
                    Debug.Log(s);
            }*/
            if (activeType == ActiveType.None || activeFlow == null) return;
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    DrawList(reorderableList);
                    DrawCommandInspector(LastSelectedCommand);
                }

                Event e = Event.current;
                var commandListRect = new Rect(0, 0, window.position.size.x * WindowSplitRatio, window.position.size.y);
                bool isOnCmdList = commandListRect.Contains(e.mousePosition);

                HandleShortcutKey(e);
                HandleMenu(e, isOnCmdList);
                if (isOnCmdList)
                {
                    reorderableList.GrabKeyboardFocus();
                }

                if (scope.changed)
                {
                    SetCommandToFlowchart(commandList);
                }
            }


            void DrawList(ReorderableList reorderableList)
            {
                if (reorderableList == null) return;
                using (var scroll = new GUILayout.ScrollViewScope(
                    listScrollPos, EditorStyles.helpBox, GUILayout.Width(window.position.size.x * WindowSplitRatio)))
                {
                    listScrollPos = scroll.scrollPosition;
                    reorderableList.DoLayoutList();
                }
            }

            void DrawCommandInspector(CommandData cmdData)
            {
                if (cmdData == null) return;
                using (var scroll = new GUILayout.ScrollViewScope(commandScrollPos, EditorStyles.helpBox))
                {
                    commandScrollPos = scroll.scrollPosition;
                    UnityEditor.Editor.CreateEditor(cmdData).OnInspectorGUI();
                }
            }

            void HandleShortcutKey(Event e)
            {
                if (e.type == EventType.KeyDown && e.control
                 && selectedIndices != null && selectedIndices.Count != 0)
                {
                    if (e.keyCode == KeyCode.C)
                    {
                        copiedCommandList = CopyFrom(selectedIndices);
                    }
                    else if (e.keyCode == KeyCode.V)
                    {
                        PasteFrom(copiedCommandList);
                    }
                    else if (e.keyCode == KeyCode.D)
                    {
                        DuplicateFrom(selectedIndices);
                    }
                }
            }

            void HandleMenu(Event e, bool containsRect)
            {
                if (e.type == EventType.ContextClick && e.button == 1 && containsRect)
                {
                    GenericMenu menu = new();
                    if (selectedIndices.Count == 0)
                    {
                        menu.AddDisabledItem(new GUIContent("Add"));
                        menu.AddDisabledItem(new GUIContent("Remove"));
                        menu.AddDisabledItem(new GUIContent("Copy"));
                        menu.AddDisabledItem(new GUIContent("Paste"));
                        menu.AddSeparator(string.Empty);
                        menu.AddDisabledItem(new GUIContent("Edit Script"));
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Add"), false, () => Add());
                        menu.AddItem(new GUIContent("Remove"), false, () => Remove());
                        menu.AddItem(new GUIContent("Copy"), false, () => copiedCommandList = CopyFrom(selectedIndices));
                        if (copiedCommandList == null || copiedCommandList.Where(c => c != null).Count() == 0)
                        {
                            menu.AddDisabledItem(new GUIContent("Paste"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Paste"), false, () => PasteFrom(copiedCommandList));
                        }
                        menu.AddSeparator(string.Empty);
                        if (LastSelectedCommand.GetCommandBase() == null)
                        {
                            menu.AddDisabledItem(new GUIContent("Edit Script"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Edit Script"), false, () =>
                            {
                                var cmdName = LastSelectedCommand.GetName();
                                if (FlowchartEditorUtility.TryGetScriptPath(cmdName, out var scriptPath))
                                {
                                    var scriptAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
                                    AssetDatabase.OpenAsset(scriptAsset, 7); // おおよその行数をカーソル
                                }
                                else
                                {
                                    Log($"スクリプト\"{cmdName}\"を開くのに失敗しました\n" +
                                    "クラス名とファイル名が一致していない可能性があります", true);
                                }
                            });
                        }
                    }

                    if (menu.GetItemCount() > 0)
                    {
                        menu.ShowAsContext();
                        e.Use();
                    }
                }
            }
        }

        List<CommandData> CopyFrom(IEnumerable<int> selectedIndices, bool callLog = true)
        {
            List<CommandData> copiedList = selectedIndices.Select(i => commandList[i]).ToList();
            if (callLog)
            {
                if (copiedList == null || copiedList.Count == 0)
                {
                    Log("コマンドのコピーに失敗しました", true);
                }
                else
                {
                    Log("コマンドをコピーしました");
                }
            }
            Event.current?.Use();
            return copiedList;
        }
        void PasteFrom(List<CommandData> copiedList, bool callLog = true)
        {
            if (copiedList == null || copiedList.Where(c => c != null).Count() == 0)
            {
                if (callLog)
                    Log("ペーストに失敗しました\nコピーされているコマンドがありません", true);
                return;
            }

            SetRecordUndoName(OperateType.Paste);

            List<CommandData> createdList = new(copiedList.Count);
            for (int i = 0; i < copiedList.Count; i++)
            {
                var cmd = copiedList[^(i + 1)];
                if (cmd == null) continue;

                CommandData createdCmd = null;
                if (activeType == ActiveType.Executor)
                {
                    createdCmd = UnityEngine.Object.Instantiate(cmd);
                }
                else if (activeType == ActiveType.Data)
                {
                    createdCmd = FlowchartEditorUtility.DuplicateSubCommandData(activeFlowchartData, cmd);
                }
                createdList.Add(createdCmd);
            }

            Undo.RecordObject(window, "Paste List");

            int currentIndex = LastSelectedIndex.GetValueOrDefault();
            foreach (var c in createdList)
            {
                commandList.Insert(currentIndex + 1, c);
            }

            SelectCommand(null);
            updatedIndices.Clear();
            foreach (var c in createdList)
            {
                SelectCommand(c, append: true);
                updatedIndices.Add(commandList.IndexOf(c));
            }

            SetCommandToFlowchart(commandList);
            Event.current?.Use();

            if (callLog)
                Log("コマンドをペーストしました");
        }
        void DuplicateFrom(List<int> selectedIndices, bool callLog = true)
        {
            List<CommandData> copiedList = CopyFrom(selectedIndices, false);
            PasteFrom(copiedList, false);
            SetRecordUndoName(OperateType.Duplicate);
            if (callLog)
                Log("コマンドを複製しました");
        }

        void Add(ReorderableList list = null)
        {
            SetRecordUndoName(OperateType.Add);

            CommandData createdCmd = null;
            if (activeType == ActiveType.Executor)
            {
                createdCmd = ScriptableObject.CreateInstance<CommandData>();
            }
            else if (activeType == ActiveType.Data)
            {
                createdCmd = FlowchartEditorUtility.CreateSubCommandData(activeFlowchartData, $"Command_{activeFlowchartData.name}");
            }

            Undo.RecordObject(window, "Add List");
            updatedIndices.Clear();
            int insertIndex = LastSelectedIndex == null ?
                commandList.Count :
                (LastSelectedIndex.GetValueOrDefault() + 1);
            commandList.Insert(insertIndex, createdCmd);
            updatedIndices.Add(insertIndex);
            SelectCommand(createdCmd);
            reorderableList.GrabKeyboardFocus();
        }

        void Remove(ReorderableList list = null)
        {
            Undo.IncrementCurrentGroup(); // Deleteキーの押しっぱなし対策
            SetRecordUndoName(OperateType.Remove);

            if (activeType == ActiveType.Data)
            {
                for (int i = 0; i < selectedIndices.Count; i++)
                {
                    var cmd = commandList[selectedIndices[^(i + 1)]];
                    Undo.DestroyObjectImmediate(cmd);
                }
                AssetDatabase.SaveAssets();
            }

            Undo.RecordObject(window, "Remove List");
            updatedIndices.Clear();

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                // selectedCommandListの後ろから適用する
                int removeIndex = selectedIndices[^(i + 1)];
                var cmd = commandList[removeIndex];
                commandList.Remove(cmd);
                updatedIndices.Add(removeIndex);

                if (i == selectedIndices.Count - 1)
                {
                    selectedIndices.Clear();
                    SelectCommand(removeIndex - 1);
                }
            }
        }

        /// <summary>
        /// コマンドを選択状態にします
        /// </summary>
        void SelectCommand(CommandData command, bool append = false)
        {
            if (append == false)
            {
                selectedIndices.Clear();
            }
            if (command == null)
            {
                reorderableList.ClearSelection();
                return;
            }
            selectedIndices.Add(commandList.IndexOf(command));
            if (selectedIndices.Count >= 2)
                selectedIndices.Sort();
            reorderableList.Select(commandList.IndexOf(command), append);
        }
        void SelectCommand(int index, bool append = false)
        {
            if (commandList.Count == 0) return;
            SelectCommand(commandList[Mathf.Clamp(index, 0, commandList.Count - 1)], append);
        }


        ReorderableList CreateReorderableList()
        {
            return new ReorderableList(
                commandList, typeof(CommandData),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                multiSelect = true,
                onAddCallback = Add,
                onRemoveCallback = Remove,
                onSelectCallback = OnSelect,
                onReorderCallbackWithDetails = OnReorder,
                elementHeightCallback = GetElementHeight,
                drawElementCallback = DrawElement,
                drawElementBackgroundCallback = DrawElementBackground,
            };


            void OnSelect(ReorderableList list)
            {
                selectedIndices.Clear();
                foreach (var i in list.selectedIndices)
                {
                    SelectCommand(i, append: true);
                }
            }

            void OnReorder(ReorderableList list, int fromIndex, int toIndex)
            {
                SetRecordUndoName(OperateType.Reorder);
                Undo.RecordObject(window, "Reorder List");
                reorderFromIndex = fromIndex;
                reorderToIndex = toIndex;
                SetObjectDirty();
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var cmd = commandList[index];

                var style = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                };
                var tmpColor = GUI.color;
                GUI.color = Color.black;

                string cmdName = cmd.GetName() ?? "Null";
                EditorGUI.LabelField(rect, $"<size=12>{cmdName}</size>", style); // コマンド名の表示

                string summary = cmd.GetSummary();
                if (string.IsNullOrEmpty(summary) == false)
                {
                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, rect.width, rect.height),
                        $"<size=10>{TagUtility.RemoveSizeTag(cmd.GetSummary())}</size>", style); // サマリーの表示
                }

                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var cmd = commandList[index];

                Color color = cmd.Enabled ?
                    cmd.GetCommandColor() :
                    color = new Color(0.7f, 0.7f, 0.7f, 1f);
                EditorGUI.DrawRect(rect, color);

                if (isActive)
                {
                    EditorGUI.DrawRect(rect, CommandSelectedColor);
                }

                var tmpColor = GUI.color;
                GUI.color = Color.black;
                GUI.Box(new Rect(rect.x, rect.y, rect.width, 1), string.Empty);
                GUI.color = tmpColor;
            }

            float GetElementHeight(int index)
            {
                return 30;
            }
        }

        #endregion
    }
}
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
    public class FlowchartEditorWindow : EditorWindow
    {
        enum ActiveMode
        {
            None,
            Executor,
            Data,
        }

        /// <summary>
        /// MonoBehaviourとScriptableObjectのどちらのFlowchartがフォーカスされているか
        /// </summary>
        [SerializeField] ActiveMode activeMode;

        [SerializeField] Flowchart activeFlowchart;
        GameObject activeExecutorObj;
        [SerializeField] FlowchartData activeFlowchartData;

        /// <summary>
        /// FlowchartExecutorの選択状態を、エディタを閉じた後も記録するための名前
        /// </summary>
        [SerializeField] string selectedName;

        /// <summary>
        /// 表示用のコマンドリスト
        /// </summary>
        [SerializeField] List<CommandData> commandList;

        [SerializeField] ReorderableList reorderableList;
        List<CommandData> selectedCommandList;
        CommandData LastSelectedCommand
        {
            get
            {
                if (selectedCommandList == null || selectedCommandList.Count == 0) return null;
                return selectedCommandList?.Last();
            }
        }

        List<CommandData> copiedCommandList;

        Vector2 listScrollPos;
        Vector2 commandScrollPos;

        /// <summary>
        /// コマンドのAddをUndoする際、元々選択していたコマンドのインデックスが知りたいので導入 
        /// </summary>
        int addUndoIndex;

        /// <summary>
        /// コマンドのRemoveをUndoする際、削除したコマンドが知りたいので導入
        /// </summary>
        List<CommandData> removedCommands;

        static readonly float WindowSplitRatio = 0.5f;

        [MenuItem("Tools/Novel System/Flowchart Window")]
        public static void OpenEditorWindow()
        {
            GetWindow<FlowchartEditorWindow>("Flowchart Editor", typeof(SceneView));
        }

        void OnEnable()
        {
            Init();


            void Init()
            {
                EditorSceneManager.sceneOpened -= OnSceneChanged;
                EditorSceneManager.sceneOpened += OnSceneChanged;
                EditorApplication.playModeStateChanged -= OnChangePlayMode;
                EditorApplication.playModeStateChanged += OnChangePlayMode;
                Undo.undoRedoEvent -= OnUndo;
                Undo.undoRedoEvent += OnUndo;

                if (activeMode == ActiveMode.Executor)
                {
                    var executor = GameObject.FindObjectsByType<FlowchartExecutor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                        .FirstOrDefault(e => e.name == selectedName);
                    if (executor != null)
                    {
                        activeExecutorObj = executor.gameObject;
                    }
                }

                commandList = new();
                selectedCommandList = new();
                removedCommands = new();
                UpdateFlowchartObjectAndWindow();
                SetActiveFlowchart();
            }
        }
        void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneChanged;
            EditorApplication.playModeStateChanged -= OnChangePlayMode;
            Undo.undoRedoEvent -= OnUndo;
        }

        // 選択しているオブジェクトが変更された際に呼ばれる
        void OnSelectionChange()
        {
            UpdateFlowchartObjectAndWindow();
        }

        void OnChangePlayMode(PlayModeStateChange state)
        {
            // モードが変化した際に処理をする
            if (state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode)
            {
                SetActiveFlowchart();
            }
        }

        void OnSceneChanged(Scene _, OpenSceneMode __)
        {
            // シーン変更時、Executorのエディタをリセット
            if (activeMode == ActiveMode.Executor)
            {
                selectedName = string.Empty;
                activeMode = ActiveMode.None;
                activeExecutorObj = null;
                activeFlowchart = null;

                reorderableList = null;
                selectedCommandList = null;
                copiedCommandList?.Clear();

                listScrollPos = Vector2.zero;
                commandScrollPos = Vector2.zero;

                addUndoIndex = 0;
                removedCommands = new();
            }
        }

        void UpdateFlowchartObjectAndWindow()
        {
            bool isUpdated = UpdateFlowchartObject();
            if (isUpdated)
                UpdateWindow();


            bool UpdateFlowchartObject()
            {
                bool isChanged = false;

                GameObject activeObj = Selection.activeGameObject;
                if (activeObj != null && activeObj.TryGetComponent<FlowchartExecutor>(out var executor))
                {
                    // Executorがヒットした
                    isChanged = activeFlowchart != executor.Flowchart;
                    activeMode = ActiveMode.Executor;
                    activeExecutorObj = activeObj;
                    activeFlowchart = executor.Flowchart;
                }
                else if (Selection.activeObject != null)
                {
                    var data = Selection.GetFiltered<FlowchartData>(SelectionMode.Assets).FirstOrDefault();
                    if (data != null)
                    {
                        // Dataがヒットした
                        isChanged = activeFlowchart != data.Flowchart;
                        activeMode = ActiveMode.Data;
                        activeFlowchartData = data;
                        activeFlowchart = data.Flowchart;
                    }
                }

                if (isChanged)
                {
                    commandList = activeFlowchart.GetCommandDataList();
                    selectedCommandList = new();
                    listScrollPos = Vector2.zero;
                    commandScrollPos = Vector2.zero;
                    selectedName = Selection.activeObject.name;
                }
                return isChanged;
            }
        }

        // Flowchartの表示を更新する
        void SetActiveFlowchart()
        {
            if (activeMode == ActiveMode.Executor && activeExecutorObj != null)
            {
                activeFlowchart = activeExecutorObj.GetComponent<FlowchartExecutor>().Flowchart;
            }
            else if (activeMode == ActiveMode.Data && activeFlowchartData != null)
            {
                activeFlowchart = activeFlowchartData.Flowchart;
            }

            if (activeFlowchart != null)
                commandList = activeFlowchart.GetCommandDataList();
            UpdateWindow();
        }

        void UpdateWindow()
        {
            if (activeFlowchart == null) return;
            AssetDatabase.SaveAssets();
            reorderableList = CreateReorderableList();
            Repaint();
        }

        void OnUndo(in UndoRedoInfo info)
        {
            UndoType operateType = GetUndoRedoType(info);
            if (activeFlowchart == null || operateType == UndoType.None) return;

            List<CommandData> oldList = null;
            if (operateType is UndoType.Remove && activeMode == ActiveMode.Data && info.isRedo == false)
            {
                oldList = new(activeFlowchartData.Flowchart.GetCommandDataList());
                AssetDatabase.SaveAssets();
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(activeFlowchartData))
                    .Where(x => AssetDatabase.IsSubAsset(x))
                    .ToArray();

                var list = activeFlowchartData.Flowchart.GetCommandDataList();
                for (int i = 0; i < list.Count; i++)
                {
                    for (int k = 0; k < subAssets.Length; k++)
                    {
                        // Unity特有の偽nullに対処するため意味の分からないコードを書いている
                        CommandData convertedCmd = subAssets[k] as CommandData;
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
                AssetDatabase.SaveAssets();
                SetCommandToFlowchart(commandList);
            }
            reorderableList = CreateReorderableList();

            if (operateType is UndoType.Add or UndoType.Paste or UndoType.Duplicate
             || operateType is UndoType.Remove && info.isRedo)
            {
                var cmd = LastSelectedCommand;
                SelectCommand(null);
                if (cmd == null || commandList.Contains(cmd) == false)
                {
                    SelectCommand(addUndoIndex - 1);
                }
                else
                {
                    SelectCommand(cmd);
                }
            }
            else if (operateType == UndoType.Remove)
            {
                // Undo前に選択していたコマンドを選択状態にする
                SelectCommand(null);
                IEnumerable<CommandData> removedCmds = activeMode == ActiveMode.Executor ?
                    removedCommands :
                    commandList.Except(oldList.Where(c => c != null));
                foreach (var c in removedCmds)
                {
                    SelectCommand(c, append: true);
                }
            }
            Repaint();
        }

        void OnGUI()
        {
            if (activeMode == ActiveMode.None || activeFlowchart == null) return;
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    DrawList(reorderableList);
                    DrawCommandInspector(LastSelectedCommand);
                }

                if (scope.changed)
                {
                    SetCommandToFlowchart(commandList);
                }
            }
        }

        void SetCommandToFlowchart(List<CommandData> list)
        {
            if (activeFlowchart == null) return;
            activeFlowchart.SetCommandDataList(list);
            if (activeMode == ActiveMode.Executor)
            {
                EditorUtility.SetDirty(activeExecutorObj);
            }
            else if (activeMode == ActiveMode.Data)
            {
                EditorUtility.SetDirty(activeFlowchartData);
            }
        }

        void DrawList(ReorderableList reorderableList)
        {
            if (reorderableList == null) return;
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control && selectedCommandList.Count != 0)
            {
                if (e.keyCode == KeyCode.C && selectedCommandList != null)
                {
                    CopyFrom(selectedCommandList);
                }
                else if (e.keyCode == KeyCode.V && copiedCommandList != null)
                {
                    PasteFrom(copiedCommandList);
                }
                else if (e.keyCode == KeyCode.D && selectedCommandList != null)
                {
                    DuplicateFrom(selectedCommandList);
                }
            }

            if (Event.current.type == EventType.ContextClick && Event.current.button == 1)
            {
                var mousePos = Event.current.mousePosition;
                var commandListRect = new Rect(0, 0, position.size.x * WindowSplitRatio, position.size.y);
                if (commandListRect.Contains(mousePos))
                {
                    GenericMenu menu = new();
                    if (selectedCommandList.Count == 0)
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
                        menu.AddItem(new GUIContent("Copy"), false, () => CopyFrom(selectedCommandList));
                        if (copiedCommandList == null)
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
                            });
                        }
                    }
                    if (menu.GetItemCount() > 0)
                    {
                        menu.ShowAsContext();
                        Event.current.Use();
                    }
                }
            }

            using (var scroll = new GUILayout.ScrollViewScope(
                listScrollPos, EditorStyles.helpBox, GUILayout.Width(position.size.x * WindowSplitRatio)))
            {
                listScrollPos = scroll.scrollPosition;
                reorderableList.DoLayoutList();
            }
        }

        void DrawCommandInspector(CommandData command)
        {
            using (var scroll = new GUILayout.ScrollViewScope(commandScrollPos, EditorStyles.helpBox))
            {
                commandScrollPos = scroll.scrollPosition;
                if (command == null) return;
                UnityEditor.Editor.CreateEditor(command).OnInspectorGUI();
            }
        }

        void CopyFrom(List<CommandData> selectedCommandList, bool callLog = true)
        {
            Event.current?.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                copiedCommandList = new List<CommandData>(selectedCommandList);
            }
            if (callLog)
            {
                if (copiedCommandList == null || copiedCommandList.Count == 0)
                {
                    Log("コマンドのコピーに失敗しました", true);
                }
                else
                {
                    Log("コマンドをコピーしました");
                }
            }
        }
        void PasteFrom(List<CommandData> copiedCommandList, bool callLog = true)
        {
            if (callLog && (copiedCommandList == null || copiedCommandList.Where(c => c != null).Count() == 0))
            {
                Log("ペーストに失敗しました\nコマンドがコピーされていません", true);
                return;
            }

            SetRecordUndoName(UndoType.Paste);
            int currentIndex = commandList.IndexOf(LastSelectedCommand);
            SelectCommand(null);
            Event.current?.Use();
            if (GUIUtility.keyboardControl <= 0) return;

            List<CommandData> createdList = new(copiedCommandList.Count);
            for (int i = 0; i < copiedCommandList.Count; i++)
            {
                var cmdData = copiedCommandList[^(i + 1)];
                if (cmdData == null) continue;

                CommandData createdCmd = null;
                if (activeMode == ActiveMode.Executor)
                {
                    createdCmd = Instantiate(cmdData);
                }
                else if (activeMode == ActiveMode.Data)
                {
                    createdCmd = FlowchartEditorUtility.DuplicateSubCommandData(activeFlowchartData, cmdData);
                }
                createdList.Add(createdCmd);
            }

            Undo.RecordObject(this, "Paste List");

            foreach (var c in createdList)
            {
                commandList.Insert(currentIndex + 1, c);
            }
            foreach (var c in createdList)
            {
                SelectCommand(c, append: true);
            }

            if (callLog)
                Log("コマンドをペーストしました");
        }
        void DuplicateFrom(List<CommandData> selectedCommandList, bool callLog = true)
        {
            CopyFrom(selectedCommandList, false);
            PasteFrom(copiedCommandList, false);
            SetRecordUndoName(UndoType.Duplicate);
            if (callLog)
                Log("コマンドを複製しました");
        }

        void Add(ReorderableList list = null)
        {
            SetRecordUndoName(UndoType.Add);

            CommandData createdCmd = null;
            if (activeMode == ActiveMode.Executor)
            {
                createdCmd = CreateInstance<CommandData>();
            }
            else if (activeMode == ActiveMode.Data)
            {
                createdCmd = FlowchartEditorUtility.CreateSubCommandData(
                    activeFlowchartData, $"{nameof(CommandData)}_{activeFlowchartData.name}");
            }

            Undo.RecordObject(this, "Add List");

            int insertIndex = LastSelectedCommand == null ?
                commandList.Count :
                (commandList.IndexOf(LastSelectedCommand) + 1);
            commandList.Insert(insertIndex, createdCmd);
            SelectCommand(createdCmd);
            reorderableList.GrabKeyboardFocus();
        }

        void Remove(ReorderableList list = null)
        {
            SetRecordUndoName(UndoType.Remove);

            if (activeMode == ActiveMode.Data)
            {
                for (int i = 0; i < selectedCommandList.Count; i++)
                {
                    var cmd = selectedCommandList[^(i + 1)];
                    Undo.DestroyObjectImmediate(cmd);
                }
                AssetDatabase.SaveAssets();
            }

            Undo.RecordObject(this, "Remove List");
            removedCommands.Clear();
            for (int i = 0; i < selectedCommandList.Count; i++)
            {
                // selectedCommandListの後ろから適用する
                var cmd = selectedCommandList[^(i + 1)];
                int removeIndex = commandList.IndexOf(cmd);
                commandList.Remove(cmd);
                removedCommands.Add(cmd);

                if (i != selectedCommandList.Count - 1) continue;
                selectedCommandList.Clear();
                SelectCommand(removeIndex - 1);
            }
        }

        /// <summary>
        /// コマンドを選択状態にします
        /// </summary>
        void SelectCommand(CommandData command, bool append = false)
        {
            if (append == false)
            {
                selectedCommandList.Clear();
            }
            if (command == null)
            {
                reorderableList.ClearSelection();
                return;
            }
            selectedCommandList.Add(command);
            reorderableList.Select(commandList.IndexOf(command), append);
            addUndoIndex = commandList.IndexOf(command);
        }
        void SelectCommand(int index, bool append = false)
        {
            if (commandList.Count == 0) return;
            SelectCommand(commandList[Mathf.Clamp(index, 0, commandList.Count - 1)], append);
        }

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

        enum UndoType
        {
            None,
            Add,
            Remove,
            Paste,
            Duplicate,
        }

        void SetRecordUndoName(UndoType undoType)
        {
            string recordName = undoType switch
            {
                UndoType.Add => "Add Command",
                UndoType.Remove => "Remove Command",
                UndoType.Paste => "Paste Command",
                UndoType.Duplicate => "Duplicate Command",
                _ => throw new Exception("UndoRedoType is None")
            };
            Undo.SetCurrentGroupName(recordName);
        }

        UndoType GetUndoRedoType(UndoRedoInfo info)
        {
            return info.undoName switch
            {
                "Add Command" => UndoType.Add,
                "Remove Command" => UndoType.Remove,
                "Paste Command" => UndoType.Paste,
                "Duplicate Command" => UndoType.Duplicate,
                _ => UndoType.None
            };
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
                onReorderCallback = OnReorder,
                drawElementCallback = DrawElement,
                drawElementBackgroundCallback = DrawElementBackground,
                elementHeightCallback = GetElementHeight,
            };

            void OnSelect(ReorderableList list)
            {
                selectedCommandList.Clear();
                foreach (var i in list.selectedIndices)
                {
                    selectedCommandList.Add(commandList[i]);
                }
                addUndoIndex = list.selectedIndices.Last();
            }

            void OnReorder(ReorderableList list)
            {
                if (activeMode == ActiveMode.Executor)
                {
                    EditorUtility.SetDirty(activeExecutorObj);
                }
                else if (activeMode == ActiveMode.Data)
                {
                    EditorUtility.SetDirty(activeFlowchartData);
                }
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
                EditorGUI.LabelField(rect, $"<size=12>{cmdName}</size>", style);
                EditorGUI.LabelField(new Rect(rect.x + 90, rect.y, rect.width, rect.height),
                    $"<size=10>{TagUtility.RemoveSizeTag(cmd.GetSummary())}</size>", style);

                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var cmd = commandList[index];

                var color = (Color)default;
                if (isActive)
                {
                    color = new Color(0.2f, 0.85f, 0.95f, 1f);
                }
                else if (cmd.Enabled)
                {
                    color = cmd.GetCommandColor();
                }
                else
                {
                    color = new Color(0.7f, 0.7f, 0.7f, 1f);
                }
                EditorGUI.DrawRect(rect, color);

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
    }
}
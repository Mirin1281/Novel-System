using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

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

        Flowchart activeFlowchart;
        GameObject activeExecutorObj;
        [SerializeField] FlowchartData activeFlowchartData;
        
        [SerializeField] string selectedName;

        /// <summary>
        /// 表示用のコマンドリスト
        /// </summary>
        [SerializeField] List<CommandData> commandList = new();
        ReorderableList reorderableList;
        CommandData lastSelectedCommand;

        List<CommandData> selectedCommandList;
        List<CommandData> copiedCommandList;
        List<int> beforeSelectedIndices;

        Vector2 listScrollPos;
        Vector2 commandScrollPos;

        static readonly float WindowSplitRatio = 0.5f;

        void OnEnable()
        {
            Init();
        }
        void OnDisable()
        {
            EditorSceneManager.sceneOpened -= ClearList;
            EditorApplication.playModeStateChanged -= UpdateOnChangeMode;
            Undo.undoRedoPerformed -= UpdateFlowchartObjectAndWindow;
        }

        /// <summary>
        /// 選択しているオブジェクトが変更された際に呼ばれる
        /// </summary>
        void OnSelectionChange()
        {
            UpdateFlowchartObjectAndWindow();
        }

        void UpdateOnChangeMode(PlayModeStateChange state)
        {
            // モードが変化した際に処理をする
            if(state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode)
            {
                SetActiveFlowchart();
                if(beforeSelectedIndices.Count == 0) return;
                lastSelectedCommand = commandList[beforeSelectedIndices[0]];
            }
        }

        void Init()
        {
            EditorSceneManager.sceneOpened -= ClearList;
            EditorSceneManager.sceneOpened += ClearList;
            EditorApplication.playModeStateChanged -= UpdateOnChangeMode;
            EditorApplication.playModeStateChanged += UpdateOnChangeMode;
            Undo.undoRedoPerformed -= UpdateFlowchartObjectAndWindow;
            Undo.undoRedoPerformed += UpdateFlowchartObjectAndWindow;

            if(activeMode == ActiveMode.Executor)
            {
                var executor = FlowchartEditorUtility.FindComponent<FlowchartExecutor>(selectedName);
                if(executor != null)
                {
                    activeExecutorObj = executor.gameObject;
                }
            }
            
            selectedCommandList = new();
            copiedCommandList = new();
            beforeSelectedIndices ??= new();
            UpdateFlowchartObjectAndWindow();
            SetActiveFlowchart();
        }

        void ClearList(Scene _, OpenSceneMode __)
        {
            if(activeMode == ActiveMode.Executor)
            {
                selectedName = string.Empty;
                activeMode = ActiveMode.None;
                activeExecutorObj = null;
                activeFlowchart = null;

                reorderableList = null;
                lastSelectedCommand = null;
                selectedCommandList = null;
                copiedCommandList = null;
                beforeSelectedIndices = null;

                listScrollPos = Vector2.zero;
                commandScrollPos = Vector2.zero;
            }
        }
        void UpdateFlowchartObjectAndWindow()
        {
            bool isUpdated = UpdateFlowchartObject();
            if(isUpdated)
                UpdateWindow();
        }

        bool UpdateFlowchartObject()
        {
            bool isUpdated = false;
            GameObject activeObj = Selection.activeGameObject;
            if (activeObj != null && activeObj.TryGetComponent<FlowchartExecutor>(out var executor))
            {
                isUpdated = !(activeExecutorObj == executor.gameObject && activeMode == ActiveMode.Executor);
                activeMode = ActiveMode.Executor;
                activeExecutorObj = activeObj;
                activeFlowchart = executor.Flowchart;
            }
            else if (Selection.activeObject != null)
            {
                var data = Selection.GetFiltered<FlowchartData>(SelectionMode.Assets).FirstOrDefault();
                if (data != null)
                {
                    isUpdated = !(activeFlowchartData == data && activeMode == ActiveMode.Data);
                    activeMode = ActiveMode.Data;
                    activeFlowchartData = data;
                    activeFlowchart = data.Flowchart;
                }
            }

            if(isUpdated)
            {
                commandList = activeFlowchart.GetCommandDataList();
                selectedCommandList = new();
                copiedCommandList ??= new();
                beforeSelectedIndices = new();
                lastSelectedCommand = null;
                listScrollPos = Vector2.zero;
                commandScrollPos = Vector2.zero;
                selectedName = Selection.activeObject.name;
            }
            return isUpdated;
        }

        void SetActiveFlowchart()
        {
            if (activeMode == ActiveMode.Executor && activeExecutorObj != null)
            {
                activeFlowchart = activeExecutorObj.GetComponent<FlowchartExecutor>().Flowchart;
            }
            else if(activeMode == ActiveMode.Data && activeFlowchartData != null)
            {
                activeFlowchart = activeFlowchartData.Flowchart;
            }
            UpdateWindow();
        }

        void UpdateWindow()
        {
            if(activeFlowchart == null) return;
            commandList = activeFlowchart.GetCommandDataList();
            reorderableList = CreateReorderableList();
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
                    DrawCommandInspector(lastSelectedCommand);
                }

                if(scope.changed)
                {
                    SetFlowchartCommand(activeFlowchart, commandList);
                }
            }
        }

        void SetFlowchartCommand(Flowchart flowchart, List<CommandData> list)
        {
            flowchart.SetCommandDataList(list);
            for (int i = 0; i < list.Count; i++)
            {
                var cmd = list[i].GetCommandBase();
                if (cmd == null) continue;
                cmd.SetFlowchart(flowchart);
                cmd.SetIndex(i);
            }
            
            if(activeMode == ActiveMode.Executor)
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
            if(reorderableList == null) return;
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control && reorderableList.HasKeyboardControl()) // ReorderableList.HasKeyboardControl()で選択中かを取得
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

            GenericMenu menu = new();
            if (Event.current.type == EventType.ContextClick && Event.current.button == 1)
            {
                var mousePos = Event.current.mousePosition;
                var commandListRect = new Rect(0, 0, position.size.x * WindowSplitRatio, position.size.y);
                if(commandListRect.Contains(mousePos))
                {
                    if (reorderableList.HasKeyboardControl())
                    {
                        menu.AddItem(new GUIContent("Add"), false, () =>
                        {
                            Add();
                        });
                        menu.AddItem(new GUIContent("Remove"), false, () =>
                        {
                            Remove();
                        });
                        menu.AddItem(new GUIContent("Copy"), false, () =>
                        {
                            CopyFrom(selectedCommandList);
                        });

                        if (copiedCommandList == null)
                        {
                            menu.AddDisabledItem(new GUIContent("Paste"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Paste"), false, () =>
                            {
                                PasteFrom(copiedCommandList);
                            });
                        }

                        menu.AddSeparator(string.Empty);

                        if (lastSelectedCommand.GetCommandBase() == null)
                        {
                            menu.AddDisabledItem(new GUIContent("Edit Script"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Edit Script"), false, () =>
                            {
                                var commandName = lastSelectedCommand.GetName();
                                var scriptPath = FlowchartEditorUtility.GetScriptPath(commandName);
                                Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(scriptPath);
                                AssetDatabase.OpenAsset(scriptAsset, 7); // おおよその行数をカーソル
                            });
                        }
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Add"));
                        menu.AddDisabledItem(new GUIContent("Remove"));
                        menu.AddDisabledItem(new GUIContent("Copy"));
                        menu.AddDisabledItem(new GUIContent("Paste"));
                        menu.AddSeparator(string.Empty);
                        menu.AddDisabledItem(new GUIContent("Edit Script"));
                    }
                }
            }

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
                Event.current.Use();
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
            if(callLog)
                Debug.Log("<color=lightblue>コマンドをコピーしました</color>");
        }
        void PasteFrom(List<CommandData> copiedCommandList, bool callLog = true)
        {
            if(copiedCommandList == null || copiedCommandList.Count == 0)
            {
                if(callLog)
                    Debug.LogWarning("ペーストに失敗しました");
                return;
            }
            Undo.RecordObject(this, "Paste Command");
            selectedCommandList.Clear();
            int currentIndex = commandList.IndexOf(lastSelectedCommand);
            Event.current?.Use();
            if (GUIUtility.keyboardControl <= 0) return;

            for (int i = 0; i < copiedCommandList.Count; i++)
            {
                var cmd = copiedCommandList[^(i + 1)];
                if (cmd == null) continue;

                var createCmd = Instantiate(cmd);
                if (activeMode == ActiveMode.Data)
                {
                    var path = FlowchartEditorUtility.GetExistFolderPath(cmd);
                    path ??= ConstContainer.COMMANDDATA_PATH;
                    var name = FlowchartEditorUtility.GetFileName(path, $"{nameof(CommandData)}_{activeFlowchartData.name}", "asset");
                    AssetDatabase.CreateAsset(createCmd, Path.Combine(path, name));
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }

                commandList.Insert(currentIndex + 1, createCmd);
                selectedCommandList.Add(createCmd);

                reorderableList.Select(currentIndex + i + 1, append: i != 0);
                if(i == copiedCommandList.Count - 1)
                {
                    lastSelectedCommand = createCmd;
                }
            }

            beforeSelectedIndices = new();
            SetFlowchartCommand(activeFlowchart, commandList);
            if (callLog)
                Debug.Log("<color=lightblue>コマンドをペーストしました</color>");
        }

        void DuplicateFrom(List<CommandData> selectedCommandList, bool callLog = true)
        {
            CopyFrom(selectedCommandList, false);
            PasteFrom(copiedCommandList, false);
            if (callLog)
                Debug.Log("<color=lightblue>コマンドを複製しました</color>");
        }

        void Add(ReorderableList list = null)
        {
            Undo.RecordObject(this, "Add Command");
            selectedCommandList.Clear();
            CommandData createCmd = activeMode switch
            {
                ActiveMode.Executor => CreateInstance<CommandData>(),
                ActiveMode.Data => FlowchartEditorUtility.CreateCommandData(
                    ConstContainer.COMMANDDATA_PATH, $"{nameof(CommandData)}_{activeFlowchartData.name}"),
                _ => throw new Exception($"{nameof(activeMode)} is None")
            };
            int insertIndex = lastSelectedCommand != null ? (commandList.IndexOf(lastSelectedCommand) + 1) : commandList.Count;
            if (commandList.Count == 0)
            {
                insertIndex = 0;
            }
            commandList.Insert(insertIndex, createCmd);
            SetFlowchartCommand(activeFlowchart, commandList);
            SelectOneCommand(createCmd);
        }

        void Remove(ReorderableList list = null)
        {
            Undo.RecordObject(this, "Remove Command");
            for (int i = 0; i < selectedCommandList.Count; i++)
            {
                var command = selectedCommandList[^(i + 1)];
                int removeIndex = commandList.IndexOf(command);
                bool isLastElementRemoved = removeIndex == commandList.Count - 1;
                commandList.Remove(command);
                
                if (activeMode == ActiveMode.Data)
                {
                    if(command == null)
                    {
                        UpdateFlowchartObjectAndWindow();
                    }
                    else
                    {
                        Undo.DestroyObjectImmediate(command);
                    }
                }

                if (i != selectedCommandList.Count - 1) continue;
                selectedCommandList.Clear();
                if (isLastElementRemoved)
                {
                    if (commandList.Count == 0) return;
                    SelectOneCommand(commandList[removeIndex - 1]);
                }
                else
                {
                    SelectOneCommand(commandList[removeIndex]);
                }
            }
        }

        /// <summary>
        /// コマンドを選択状態にします
        /// </summary>
        void SelectOneCommand(CommandData command)
        {
            lastSelectedCommand = command;
            selectedCommandList.Add(command);
            reorderableList.Select(commandList.IndexOf(command));
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
                // 最後に選択されたコマンドを調べる
                var selects = list.selectedIndices;
                for (int i = 0; i < selects.Count; i++)
                {
                    int index = selects[i];
                    if (i >= beforeSelectedIndices.Count ||
                        index != beforeSelectedIndices[i])
                    {
                        lastSelectedCommand = commandList[index];
                        break;
                    }
                }
                beforeSelectedIndices = new List<int>(selects);

                selectedCommandList.Clear();
                foreach (var i in selects)
                {
                    selectedCommandList.Add(commandList[i]);
                }
            }

            void OnReorder(ReorderableList list)
            {
                if (activeMode == ActiveMode.Executor)
                {
                    EditorUtility.SetDirty(activeExecutorObj);
                }
                else if(activeMode == ActiveMode.Data)
                {
                    EditorUtility.SetDirty(activeFlowchartData);
                }
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;

                var style = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                };
                var tmpColor = GUI.color;
                GUI.color = Color.black;

                var cmd = commandList[index];
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
                if(isFocused)
                {
                    color = new Color(0.2f, 0.85f, 0.95f, 1f);
                }
                else if(cmd.Enabled)
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
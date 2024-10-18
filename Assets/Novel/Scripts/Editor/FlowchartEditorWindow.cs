using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
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
        
        [SerializeField] int selectedID;

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
            EditorApplication.playModeStateChanged -= UpdateOnChangeMode;
            Undo.undoRedoPerformed -= UpdateFlowchartAndWindow;
        }

        /// <summary>
        /// 選択しているオブジェクトが変更された際に呼ばれる
        /// </summary>
        void OnSelectionChange()
        {
            UpdateFlowchartAndWindow();
        }

        /// <summary>
        /// エディタウィンドウをアクティブにした際に呼ばれる
        /// </summary>
        void OnFocus()
        {
            UpdateWindow();
        }

        void Init()
        {
            EditorApplication.playModeStateChanged -= UpdateOnChangeMode;
            EditorApplication.playModeStateChanged += UpdateOnChangeMode;
            Undo.undoRedoPerformed -= UpdateFlowchartAndWindow;
            Undo.undoRedoPerformed += UpdateFlowchartAndWindow;

            var obj = FlowchartEditorUtility.FindObjectFromInstanceID(selectedID);
            if(obj != null)
            {
                activeExecutorObj = obj as GameObject;
            }
            selectedCommandList = new();
            copiedCommandList = new();
            UpdateFlowchartAndWindow();
        }

        void UpdateFlowchartAndWindow()
        {
            UpdateFlowchart();
            UpdateWindow();
        }

        void UpdateFlowchart()
        {
            GameObject activeObj = Selection.activeGameObject;
            if (activeObj != null && activeObj.TryGetComponent<FlowchartExecutor>(out var executor))
            {
                activeMode = ActiveMode.Executor;
                activeExecutorObj = activeObj;
                activeFlowchart = executor.Flowchart;
                commandList = activeFlowchart.GetCommandDataList();
                selectedID = activeObj.GetInstanceID();
                beforeSelectedIndices = new();
            }
            else if (Selection.activeObject != null)
            {
                var data = Selection.GetFiltered<FlowchartData>(SelectionMode.Assets).FirstOrDefault();
                if (data != null)
                {
                    activeMode = ActiveMode.Data;
                    activeFlowchartData = data;
                    activeFlowchart = data.Flowchart;
                    commandList = activeFlowchart.GetCommandDataList();
                    selectedID = Selection.activeObject.GetInstanceID();
                    beforeSelectedIndices = new();
                }
            }
        }

        void UpdateWindow()
        {
            if (activeMode == ActiveMode.Executor && activeExecutorObj != null)
            {
                activeFlowchart = activeExecutorObj.GetComponent<FlowchartExecutor>().Flowchart;
            }
            else if(activeMode == ActiveMode.Data && activeFlowchartData != null)
            {
                activeFlowchart = activeFlowchartData.Flowchart;
            }
            
            if(activeFlowchart == null) return;
            commandList = activeFlowchart.GetCommandDataList();
            reorderableList = CreateReorderableList();
            Repaint();
        }

        void UpdateOnChangeMode(PlayModeStateChange state)
        {
            // モードが変化した際に処理をする
            if(state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode)
            {
                UpdateWindow();
                if(beforeSelectedIndices.Count == 0) return;
                lastSelectedCommand = commandList[beforeSelectedIndices[0]];
            }
        }


        void OnGUI()
        {
            if (activeMode == ActiveMode.None || activeFlowchart == null) return;

            EditorGUI.BeginChangeCheck();

            using (new GUILayout.HorizontalScope())
            {
                DrawCommandList();
                DrawCommandInspector();
            }

            if (EditorGUI.EndChangeCheck())
            {
                RefreshFlowchart(activeFlowchart);
            }
        }

        void RefreshFlowchart(Flowchart flowchart)
        {
            flowchart.SetCommandDataList(commandList);
            for (int i = 0; i < commandList.Count; i++)
            {
                var command = commandList[i].GetCommandBase();
                if (command == null) continue;
                command.SetFlowchart(flowchart);
                command.SetIndex(i);
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

        void DrawCommandList()
        {
            // ReorderableList.HasKeyboardControl()は絶対使い方間違えてるけど、
            // 簡単に選択中かを取得できるものがなぜか無かったのでこうなっている
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control && reorderableList.HasKeyboardControl())
            {
                if (e.keyCode == KeyCode.C && selectedCommandList != null)
                {
                    Copy(selectedCommandList);
                }
                else if (e.keyCode == KeyCode.V && copiedCommandList != null)
                {
                    Paste(copiedCommandList);
                }
                else if (e.keyCode == KeyCode.D && selectedCommandList != null)
                {
                    Duplicate(selectedCommandList);
                }
            }

            GenericMenu menu = new();
            if (Event.current.type == EventType.ContextClick && Event.current.button == 1)
            {
                var mousePos = Event.current.mousePosition;
                var buttonRect = new Rect(0, 0, position.size.x * WindowSplitRatio, position.size.y);
                if(buttonRect.Contains(mousePos))
                {
                    if (reorderableList.HasKeyboardControl())
                    {
                        menu.AddItem(new GUIContent("Add"), false, () =>
                        {
                            Add(reorderableList);
                        });
                        menu.AddItem(new GUIContent("Remove"), false, () =>
                        {
                            Remove(reorderableList);
                        });
                        menu.AddItem(new GUIContent("Copy"), false, () =>
                        {
                            Copy(selectedCommandList);
                        });

                        if (copiedCommandList == null)
                        {
                            menu.AddDisabledItem(new GUIContent("Paste"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Paste"), false, () =>
                            {
                                Paste(copiedCommandList);
                            });
                        }

                        menu.AddSeparator(string.Empty);

                        if (selectedCommandList.Count == 1 && lastSelectedCommand.GetCommandBase() != null)
                        {
                            menu.AddItem(new GUIContent("Edit Script"), false, () =>
                            {
                                var commandName = lastSelectedCommand.GetName();
                                var scriptPath = GetScriptPath(commandName);
                                Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(scriptPath);
                                AssetDatabase.OpenAsset(scriptAsset, 7);
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


            static string GetScriptPath(string fileName)
            {
                var assetName = fileName;
                var filterString = assetName + " t:Script";

                var path = AssetDatabase.FindAssets(filterString)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(str => string.Equals(Path.GetFileNameWithoutExtension(str),
                        assetName, StringComparison.CurrentCultureIgnoreCase));

                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning(
                        $"Edit Scriptでエラーが発生しました\n" +
                        $"開こうとしたファイル名: {fileName}.cs\n" +
                        "コマンドのクラス名とスクリプト名が一致しているか確認してください");
                    throw new FileNotFoundException();
                }
                else
                {
                    return path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                }
            }
        }

        void DrawCommandInspector()
        {
            using (var scroll = new GUILayout.ScrollViewScope(commandScrollPos, EditorStyles.helpBox))
            {
                commandScrollPos = scroll.scrollPosition;
                if (lastSelectedCommand == null) return;
                UnityEditor.Editor.CreateEditor(lastSelectedCommand).OnInspectorGUI();
            }
        }

        void Copy(List<CommandData> selectedCommandList, bool callLog = true)
        {
            Event.current?.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                copiedCommandList = new List<CommandData>(selectedCommandList);
            }
            if(callLog)
            {
                Debug.Log("<color=lightblue>コマンドをコピーしました</color>");
            }
        }
        void Paste(List<CommandData> copiedCommandList, bool callLog = true)
        {
            Undo.RecordObject(this, "Paste Command");
            selectedCommandList.Clear();
            int currentIndex = commandList.IndexOf(lastSelectedCommand);
            Event.current?.Use();
            if (GUIUtility.keyboardControl <= 0) return;

            for (int i = 0; i < copiedCommandList.Count; i++)
            {
                var command = copiedCommandList[^(i + 1)];
                if (command == null) continue;

                var createCommand = Instantiate(command);
                if (activeMode == ActiveMode.Data)
                {
                    var path = FlowchartEditorUtility.GetExistFolderPath(command);
                    if (path == null)
                    {
                        path = ConstContainer.COMMANDDATA_PATH;
                    }
                    var name = FlowchartEditorUtility.GetFileName(
                        path, $"{nameof(CommandData)}_{activeFlowchartData.name}", "asset");
                    AssetDatabase.CreateAsset(createCommand, Path.Combine(path, name));
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }

                commandList.Insert(currentIndex + 1, createCommand);
                selectedCommandList.Add(createCommand);

                if (i == 0)
                {
                    reorderableList.Select(currentIndex + 1 + i, false);
                }
                else
                {
                    reorderableList.Select(currentIndex + 1 + i, true);
                }

                if(i == copiedCommandList.Count - 1)
                {
                    lastSelectedCommand = createCommand;
                }
            }
            beforeSelectedIndices = new();

            RefreshFlowchart(activeFlowchart);
            if (callLog)
            {
                Debug.Log("<color=lightblue>コマンドをペーストしました</color>");
            }
        }

        void Duplicate(List<CommandData> selectedCommandList)
        {
            Copy(selectedCommandList, false);
            Paste(copiedCommandList, false);
            Debug.Log("<color=lightblue>コマンドを複製しました</color>");
        }

        void Add(ReorderableList list)
        {
            Undo.RecordObject(this, "Add Command");
            selectedCommandList.Clear();
            CommandData newCommand = activeMode switch
            {
                ActiveMode.Executor => CreateInstance<CommandData>(),
                ActiveMode.Data => FlowchartEditorUtility.CreateCommandData(ConstContainer.COMMANDDATA_PATH, $"{nameof(CommandData)}_{activeFlowchartData.name}"),
                _ => throw new Exception()
            };
            int insertIndex = commandList.IndexOf(lastSelectedCommand) + 1;
            if (commandList == null || commandList.Count == 0)
            {
                insertIndex = 0;
            }
            commandList.Insert(insertIndex, newCommand);
            RefreshFlowchart(activeFlowchart);
            SelectOneCommand(newCommand);
        }

        void Remove(ReorderableList list)
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
                        UpdateFlowchartAndWindow();
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
                // "最後に選択されたコマンド"を調べる
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

                var command = commandList[index];
                EditorGUI.LabelField(rect, $"<size=12>{command.GetName()  ?? "Null"}</size>", style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 90, rect.y, rect.width, rect.height),
                    $"<size=10>{TagUtility.RemoveSizeTag(command.GetSummary())}</size>", style);

                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;

                var command = commandList[index];
                var color = command.GetCommandColor();
                color.a = 1f;
                if (isFocused)
                {
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.85f, 0.95f, 1f));
                }
                else
                {
                    if (command.Enabled == false)
                    {
                        color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    }
                    EditorGUI.DrawRect(rect, color);
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
    }
}
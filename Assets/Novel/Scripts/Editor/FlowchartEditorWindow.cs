using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

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

        ActiveMode activeMode;

        Flowchart activeFlowchart;
        FlowchartData activeFlowchartData;
        GameObject activeExecutorObj;

        [SerializeField] List<CommandData> commandList = new();
        ReorderableList reorderableList;
        CommandData selectedCommand;
        CommandData copiedCommand;

        Vector2 listScrollPos;
        Vector2 commandScrollPos;

        void OnEnable()
        {
            reorderableList = CreateReorderableList();
        }

        void OnFocus()
        {
            if (activeMode == ActiveMode.Executor && activeExecutorObj != null)
            {
                var flowchartExecutor = activeExecutorObj.GetComponent<FlowchartExecutor>();
                activeFlowchart = flowchartExecutor.Flowchart;
                commandList = activeFlowchart.GetCommandDataList();
            }
            else if(activeMode == ActiveMode.Data && activeFlowchartData != null)
            {
                activeFlowchart = activeFlowchartData.Flowchart;
                commandList = activeFlowchart.GetCommandDataList();
            }
            reorderableList = CreateReorderableList();
        }

        void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var flowchartExecutor = Selection.activeGameObject.GetComponent<FlowchartExecutor>();
                if (flowchartExecutor != null)
                {
                    activeMode = ActiveMode.Executor;
                    activeExecutorObj = flowchartExecutor.gameObject;
                    activeFlowchart = flowchartExecutor.Flowchart;
                    commandList = activeFlowchart.GetCommandDataList();
                }
            }
            if (Selection.activeObject != null)
            {
                var flowchartData = Selection.GetFiltered<FlowchartData>(SelectionMode.Assets);
                if (flowchartData != null && flowchartData.Length != 0)
                {
                    activeMode = ActiveMode.Data;
                    activeFlowchartData = flowchartData[0];
                    activeFlowchart = activeFlowchartData.Flowchart;
                    commandList = activeFlowchart.GetCommandDataList();
                }
            }
            reorderableList = CreateReorderableList();
            Repaint();
        }

        void OnGUI()
        {
            if (activeMode == ActiveMode.None) return;

            EditorGUI.BeginChangeCheck();

            using (new GUILayout.HorizontalScope())
            {
                UpdateCommandList();
                UpdateCommandInspector();
            }

            if (EditorGUI.EndChangeCheck())
            {
                RefreshFlowchart();
            }
        }

        void RefreshFlowchart()
        {
            activeFlowchart.SetCommandDataList(commandList);
            for (int i = 0; i < commandList.Count; i++)
            {
                var cmd = commandList[i].GetCommandBase();
                if (cmd == null) continue;
                cmd.SetFlowchart(activeFlowchart);
                cmd.SetIndex(i);
            }
            
            if (activeMode == ActiveMode.Data)
            {
                EditorUtility.SetDirty(activeFlowchartData);
            }
        }

        void UpdateCommandList()
        {
            if (activeFlowchart == null) return;

            // ReorderableList.HasKeyboardControl()は絶対使い方間違えてるけど、
            // 簡単に選択中かを取得できるものがなぜか無かったのでこうなっている
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control && reorderableList.HasKeyboardControl())
            {
                if (e.keyCode == KeyCode.C && selectedCommand != null)
                {
                    Copy(selectedCommand);
                    Debug.Log("<color=lightblue>コマンドをコピーしました</color>");
                }
                else if (e.keyCode == KeyCode.V && copiedCommand != null)
                {
                    Paste(copiedCommand);
                    Debug.Log("<color=lightblue>コマンドをペーストしました</color>");
                }
                else if (e.keyCode == KeyCode.D && selectedCommand != null)
                {
                    Duplicate(selectedCommand);
                    Debug.Log("<color=lightblue>コマンドを複製しました</color>");
                }
            }

            GenericMenu menu = new();
            if (Event.current.type == EventType.ContextClick && Event.current.button == 1)
            {
                if(reorderableList.HasKeyboardControl())
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
                        Copy(selectedCommand);
                        Debug.Log("<color=lightblue>コマンドをコピーしました</color>");
                    });

                    if (copiedCommand == null)
                    {
                        menu.AddDisabledItem(new GUIContent("Paste"));
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Paste"), false, () =>
                        {
                            Paste(copiedCommand);
                            Debug.Log("<color=lightblue>コマンドをペーストしました</color>");
                        });
                    }
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Add"));
                    menu.AddDisabledItem(new GUIContent("Remove"));
                    menu.AddDisabledItem(new GUIContent("Copy"));
                    menu.AddDisabledItem(new GUIContent("Paste"));
                }
            }

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
                Event.current.Use();
            }

            using (GUILayout.ScrollViewScope scroll =
                new(listScrollPos, EditorStyles.helpBox, GUILayout.Width(position.size.x / 2f)))
            {
                listScrollPos = scroll.scrollPosition;
                reorderableList.DoLayoutList();
            }
        }

        void UpdateCommandInspector()
        {
            using (GUILayout.ScrollViewScope scroll = new(commandScrollPos, EditorStyles.helpBox))
            {
                commandScrollPos = scroll.scrollPosition;

                if (selectedCommand == null) return;

                UnityEditor.Editor.CreateEditor(selectedCommand).DrawDefaultInspector();

                var infoText = selectedCommand.GetCommandStatus().Info;
                if (string.IsNullOrEmpty(infoText) == false)
                {
                    EditorGUILayout.HelpBox(infoText, MessageType.Info);
                }
            }
        }

        void Copy(CommandData command)
        {
            Event.current?.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                copiedCommand = command;
            }
        }
        void Paste(CommandData copiedCommand)
        {
            Undo.RecordObject(this, "Paste Command");
            int currentIndex = reorderableList.index;
            Event.current?.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                var createCommand = Instantiate(copiedCommand);
                if(activeMode == ActiveMode.Data)
                {
                    var path = FlowchartEditorUtility.GetExistFolderPath(copiedCommand);
                    if(path == null)
                    {
                        path = ConstContainer.COMMANDDATA_PATH;
                    }
                    var name = FlowchartEditorUtility.GetFileName(
                        path, $"CommandData_{activeFlowchartData.name}", "asset");
                    AssetDatabase.CreateAsset(createCommand, Path.Combine(path, name));
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                
                commandList.Insert(currentIndex + 1, createCommand);
                selectedCommand = createCommand;
                reorderableList.Select(currentIndex + 1);

                RefreshFlowchart();
            }
        }

        void Duplicate(CommandData command)
        {
            Copy(command);
            Paste(copiedCommand);
        }

        #region ReorderableList

        ReorderableList CreateReorderableList()
        {
            return new ReorderableList(
                commandList, typeof(CommandData),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true)
            {
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
                selectedCommand = commandList[list.index];
            }

            void OnReorder(ReorderableList list)
            {
                if (activeMode == ActiveMode.Executor)
                {
                    EditorUtility.SetDirty(activeExecutorObj);
                }
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var style = new GUIStyle(EditorStyles.label);
                style.richText = true;
                var tmpColor = GUI.color;
                GUI.color = Color.black;

                var cmdStatus = commandList[index].GetCommandStatus();
                EditorGUI.LabelField(rect, $"<size=12>{cmdStatus.Name}</size>", style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 90, rect.y, rect.width, rect.height),
                    $"<size=10>{RemoveTag(cmdStatus.Summary)}</size>", style);

                GUI.color = tmpColor;


                static string RemoveTag(string text)
                {
                    string regexString = @"\<.*?\>";
                    var regex = new Regex(regexString);
                    return regex.Replace(text, string.Empty);
                }
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var cmdData = commandList[index];
                var color = cmdData.GetCommandStatus().Color;
                color.a = 1f;
                if (isFocused)
                {
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.85f, 0.95f, 1f));
                }
                else
                {
                    if (cmdData.Enabled == false)
                    {
                        color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    }
                    EditorGUI.DrawRect(rect, color);
                }
                var tmpColor = GUI.color;
                GUI.color = Color.black;
                GUI.Box(new Rect(rect.x, rect.y,rect.width, 1), string.Empty);
                GUI.color = tmpColor;
            }

            float GetElementHeight(int index)
            {
                return 30;
            }
        }

        public void Add(ReorderableList list)
        {
            Undo.RecordObject(this, "Add Command");
            CommandData newCommandData = activeMode switch
            {
                ActiveMode.Executor => CreateInstance<CommandData>(),
                ActiveMode.Data => FlowchartEditorUtility.CreateCommandData(ConstContainer.COMMANDDATA_PATH, $"CommandData_{activeFlowchartData.name}"),
                _ => throw new Exception()
            };
            int insertIndex = list.index + 1;
            if (commandList == null || commandList.Count == 0)
            {
                insertIndex = 0;
            }
            commandList.Insert(insertIndex, newCommandData);
            selectedCommand = newCommandData;
            reorderableList.Select(insertIndex);
        }

        public void Remove(ReorderableList list)
        {
            Undo.RecordObject(this, "Remove Command");
            commandList.Remove(selectedCommand);
            if (activeMode == ActiveMode.Data)
            {
                FlowchartEditorUtility.DestroyScritableObject(selectedCommand);
            }

            int removeIndex = list.index;
            bool isLastElementRemoved = removeIndex == commandList.Count;
            if (isLastElementRemoved)
            {
                if (commandList.Count == 0) return;
                selectedCommand = commandList[removeIndex - 1];
                reorderableList.Select(removeIndex - 1);
            }
            else
            {
                selectedCommand = commandList[removeIndex];
                reorderableList.Select(removeIndex);
            }
        }

        #endregion
    }
}
using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Novel
{
    public class FlowchartEditorWindow : EditorWindow
    {
        enum ActiveMode
        {
            None,
            Executor,
            Data,
        }

        Flowchart activeFlowchart;
        FlowchartData activeFlowchartData;
        ActiveMode activeMode;

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

        void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var flowchartExecutor = Selection.activeGameObject.GetComponent<FlowchartExecutor>();
                if (flowchartExecutor != null)
                {
                    activeMode = ActiveMode.Executor;
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
        }

        void UpdateCommandList()
        {
            if (activeFlowchart == null) return;

            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control)
            {
                if (e.keyCode == KeyCode.C && selectedCommand != null)
                {
                    Copy(selectedCommand);
                }
                else if (e.keyCode == KeyCode.V && copiedCommand != null)
                {
                    Paste(copiedCommand);
                }
                else if (e.keyCode == KeyCode.D && selectedCommand != null)
                {
                    Copy(selectedCommand);
                    Paste(copiedCommand);
                }
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

                Editor.CreateEditor(selectedCommand).DrawDefaultInspector();

                var infoText = selectedCommand.GetCommandStatus().Info;
                if (string.IsNullOrEmpty(infoText) == false)
                {
                    EditorGUILayout.HelpBox(infoText, MessageType.Info);
                }
            }
        }

        void Copy(CommandData command)
        {
            Event.current.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                copiedCommand = command;
            }
        }
        void Paste(CommandData copiedCommand)
        {
            Undo.RecordObject(this, "Paste Command");
            int currentIndex = reorderableList.index;
            Event.current.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                var createCommand = Instantiate(copiedCommand);
                if(activeMode == ActiveMode.Data)
                {
                    var path = FlowchartEditorUtility.GetExistFolderPath(copiedCommand);
                    var name = FlowchartEditorUtility.GetFileName(path, $"CommandData_{activeFlowchartData.name}");
                    AssetDatabase.CreateAsset(createCommand, Path.Combine(path, name));
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                
                commandList.Insert(currentIndex + 1, createCommand);
                selectedCommand = createCommand;
                reorderableList.Select(currentIndex + 1);
            }
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
                drawElementCallback = OnDrawElement,
                drawElementBackgroundCallback = DrawElementBackground,
                elementHeightCallback = GetElementHeight,
            };

            void Add(ReorderableList list)
            {
                Undo.RecordObject(this, "Add Command");
                CommandData newCommandData = activeMode switch
                {
                    ActiveMode.Executor => CreateInstance<CommandData>(),
                    ActiveMode.Data => CreateCommandData(NameContainer.COMMANDDATA_PATH, activeFlowchartData.name),
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

            void Remove(ReorderableList list)
            {
                Undo.RecordObject(this, "Remove Command");
                commandList.Remove(selectedCommand);
                if (activeMode == ActiveMode.Data)
                {
                    var deleteName = selectedCommand.name;
                    DestroyImmediate(selectedCommand, true);
                    File.Delete($"{NameContainer.COMMANDDATA_PATH}/{deleteName}.asset");
                    File.Delete($"{NameContainer.COMMANDDATA_PATH}/{deleteName}.asset.meta");
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                int removeIndex = list.index;
                bool isLastElementRemoved = removeIndex == commandList.Count;
                if (isLastElementRemoved)
                {
                    if(commandList.Count == 0) return;
                    selectedCommand = commandList[removeIndex - 1];
                    reorderableList.Select(removeIndex - 1);
                }
                else
                {
                    selectedCommand = commandList[removeIndex];
                    reorderableList.Select(removeIndex);
                }
            }

            void OnSelect(ReorderableList list)
            {
                selectedCommand = commandList[list.index];
            }

            void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var style = new GUIStyle(EditorStyles.label);
                style.richText = true;
                var tmpColor = GUI.color;
                GUI.color = Color.black;

                var cmdStatus = commandList[index].GetCommandStatus();
                EditorGUI.LabelField(rect, cmdStatus.Name, style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 75, rect.y,
                    rect.width, rect.height),
                    $"<size=10>{cmdStatus.Summary}</size>", style);

                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var cmd = commandList[index];
                var color = cmd.GetCommandStatus().Color;
                color.a = 1f;
                if (isFocused)
                {
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.85f, 0.95f, 1f));
                }
                else
                {
                    if (cmd.Enabled == false)
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

        CommandData CreateCommandData(string path, string parentDataName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var name = FlowchartEditorUtility.GetFileName(path, $"CommandData_{parentDataName}");
            var cmdData = CreateInstance<CommandData>();
            AssetDatabase.CreateAsset(cmdData, Path.Combine(path, name));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return cmdData;
        }

        #endregion
    }
}
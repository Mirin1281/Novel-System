using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Novel.Command;
using UnityEditorInternal;

namespace Novel
{
    public class FlowchartEditorWindow : EditorWindow
    {
        FlowchartExecutor activeFlowchartExecutor;
        [SerializeField] List<CommandData> commandList = new();
        ReorderableList reorderableList;
        CommandData selectedCommand;
        CommandData copiedCommand;

        Vector2 dataScrollPosition;
        Vector2 parameterScrollPosition;

        [MenuItem("Tools/Novel/Show Flowchart Editor")]
        static void CreateEditor()
        {
            GetWindow<FlowchartEditorWindow>("Flowchart Editor");
        }

        void OnEnable()
        {
            CreateReorderableList();
        }

        void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                UpdateCommandList();
                UpdateCommandInspector();
            }
        }

        void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var flowchartExecutor = Selection.activeGameObject.GetComponent<FlowchartExecutor>();
                if (flowchartExecutor != null)
                {
                    activeFlowchartExecutor = flowchartExecutor;
                    commandList = flowchartExecutor.GetCommandDataList() as List<CommandData>;
                }
            }
            CreateReorderableList();
            Repaint();
        }

        void UpdateCommandInspector()
        {
            if (activeFlowchartExecutor == null) return;
            using (GUILayout.ScrollViewScope scroll = new(parameterScrollPosition, EditorStyles.helpBox))
            {
                parameterScrollPosition = scroll.scrollPosition;

                if (selectedCommand == null) return;
                Editor.CreateEditor(selectedCommand).DrawDefaultInspector();

                var infoText = selectedCommand.GetCommandStatus().Info;
                if(string.IsNullOrEmpty(infoText) == false)
                {
                    EditorGUILayout.HelpBox(infoText, MessageType.Info);
                }
            }
        }

        void UpdateCommandList()
        {
            if (activeFlowchartExecutor == null) return;

            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.control)
            {
                if (ev.keyCode == KeyCode.C && selectedCommand != null)
                {
                    Copy(selectedCommand);
                }
                else if (ev.keyCode == KeyCode.V && copiedCommand != null)
                {
                    Paste(copiedCommand);
                }
                else if (ev.keyCode == KeyCode.D && selectedCommand != null)
                {
                    Copy(selectedCommand);
                    Paste(copiedCommand);
                }
            }

            using (GUILayout.ScrollViewScope scroll =
                new(dataScrollPosition, EditorStyles.helpBox, GUILayout.Width(position.size.x / 2f)))
            {
                dataScrollPosition = scroll.scrollPosition;
                
                if (activeFlowchartExecutor != null)
                {
                    activeFlowchartExecutor.SetCommandDataList(commandList);
                    UpdateCommandSettings();
                }
                reorderableList.DoLayoutList();
            }
        }

        void UpdateCommandSettings()
        {
            int i = 0;
            foreach(var cmdData in commandList)
            {
                var cmd = cmdData.GetCommandBase();
                if (cmd == null) continue;
                cmd.SetFlowchart(activeFlowchartExecutor);
                cmd.SetIndex(i);
                i++;
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
                commandList.Insert(currentIndex, createCommand);
            }
        }

        #region ReorderableList

        void CreateReorderableList()
        {
            reorderableList = new ReorderableList(commandList, typeof(CommandData))
            {
                onAddCallback = Add,
                onRemoveCallback = Remove,
                onSelectCallback = OnSelect,
                drawElementCallback = OnDrawElement,
                drawElementBackgroundCallback = DrawElementBackground,
                drawHeaderCallback = DrawHeader,
                elementHeightCallback = GetElementHeight,
            };

            void Add(ReorderableList list)
            {
                Undo.RecordObject(this, "Add Command");
                var newCommand = CreateInstance<CommandData>();
                int insertIndex = list.index + 1;
                commandList.Insert(insertIndex, newCommand);
                selectedCommand = newCommand;
                reorderableList.Select(insertIndex);
            }

            void Remove(ReorderableList list)
            {
                Undo.RecordObject(this, "Remove Command");
                commandList.Remove(selectedCommand);
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
                UpdateCommandInspector();
            }

            void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                var style = new GUIStyle(EditorStyles.label);
                style.richText = true;
                var tmpColor = GUI.color;
                GUI.color = Color.black;

                var cmdStatus = commandList[index].GetCommandStatus();
                EditorGUI.LabelField(rect, cmdStatus.Name, style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 80, rect.y,
                    rect.width, rect.height),
                    $"<size=10>{cmdStatus.Summary}</size>", style);

                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
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
            }

            void DrawHeader(Rect rect)
            {
                EditorGUI.LabelField(rect, "CommandList");
            }

            float GetElementHeight(int index)
            {
                return 30;
            }
        }
        #endregion

    }
}
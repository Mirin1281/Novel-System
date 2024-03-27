using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Novel.Command;
using UnityEditorInternal;

namespace Novel
{
    public class FlowchartEditorWindow : EditorWindow
    {
        [SerializeField] List<CommandData> commandList = new();
        ReorderableList reorderableList;
        CommandData selectedCommand;
        FlowchartExecutor activeFlowchartExecutor;

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
                    commandList = flowchartExecutor.CommandDataList as List<CommandData>;
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
                reorderableList.DoLayoutList();
                if (activeFlowchartExecutor != null)
                {
                    activeFlowchartExecutor.SetCommandList(commandList);
                }
            }
        }

        void Copy(CommandData command)
        {
            Event.current.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                copiedCommand = Instantiate(command);
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
                commandList.Insert(list.index + 1, newCommand);
                selectedCommand = newCommand;
                reorderableList.Select(list.index + 1);
            }

            void Remove(ReorderableList list)
            {
                Undo.RecordObject(this, "Remove Command");
                commandList.Remove(selectedCommand);
                int removeIndex = list.index;
                bool isLastRemoved = removeIndex == commandList.Count;
                if (isLastRemoved)
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
                if (index < 0 || commandList.Count <= index) return;
                var style = new GUIStyle(EditorStyles.label);
                style.richText = true;
                var tmpColor = GUI.color;
                GUI.color = Color.black;
                string commandName = commandList[index].GetCommandStatus().Name;
                string summary = commandList[index].GetCommandStatus().Summary;
                EditorGUI.LabelField(rect, commandName, style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 80, rect.y,
                    rect.width, rect.height),
                    $"<size=10>{summary}</size>", style);
                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;
                var color = commandList[index].GetCommandStatus().Color;
                color.a = 0.7f;
                if (isFocused)
                {
                    EditorGUI.DrawRect(rect, color * new Color(0, 1, 1, 0.8f));
                }
                else
                {
                    var enabled = commandList[index].Enabled;
                    if (enabled == false)
                    {
                        EditorGUI.DrawRect(rect, new Color(0.6f, 0.6f, 0.6f, 0.8f));
                        return;
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
    }
}
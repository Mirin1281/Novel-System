using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    using FlowchartType = FlowExecuteCommand.FlowchartType;

    [CustomPropertyDrawer(typeof(FlowExecuteCommand))]
    public class FlowExecuteCommandDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUILayout.Space(-10);

            var flowchartTypeProp = property.FindPropertyRelative("flowchartType");
            EditorGUILayout.PropertyField(flowchartTypeProp, new GUIContent("FlowchartType"));

            if((FlowchartType)flowchartTypeProp.enumValueIndex == FlowchartType.Executor)
            {
                var flowchartExecutorProp = property.FindPropertyRelative("flowchartExecutor");
                EditorGUILayout.PropertyField(flowchartExecutorProp, new GUIContent("FlowchartExecutor"));
            }
            else if((FlowchartType)flowchartTypeProp.enumValueIndex == FlowchartType.Data)
            {
                var flowchartDataProp = property.FindPropertyRelative("flowchartData");
                EditorGUILayout.PropertyField(flowchartDataProp, new GUIContent("FlowchartData"));
            }

            var commandIndexProp = property.FindPropertyRelative("commandIndex");
            EditorGUILayout.PropertyField(commandIndexProp, new GUIContent("CommandIndex"));

            var isAwaitNestProp = property.FindPropertyRelative("isAwaitNest");
            EditorGUILayout.PropertyField(isAwaitNestProp, new GUIContent("IsAwaitNest"));
        }
    }
}
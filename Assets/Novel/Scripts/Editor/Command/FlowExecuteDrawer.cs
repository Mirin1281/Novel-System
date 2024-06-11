using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    using FlowchartType = FlowExecute.FlowchartType;

    [CustomPropertyDrawer(typeof(FlowExecute))]
    public class FlowExecuteDrawer : CommandBaseDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += GetHeight(10);
            EditorGUI.BeginProperty(position, label, property);

            var flowchartTypeProp = property.FindPropertyRelative("flowchartType");
            EditorGUI.PropertyField(position, flowchartTypeProp, new GUIContent("FlowchartType"));
            position.y += GetHeight();

            if ((FlowchartType)flowchartTypeProp.enumValueIndex == FlowchartType.Executor)
            {
                var flowchartExecutorProp = property.FindPropertyRelative("flowchartExecutor");
                EditorGUI.PropertyField(position, flowchartExecutorProp, new GUIContent("FlowchartExecutor"));
            }
            else if((FlowchartType)flowchartTypeProp.enumValueIndex == FlowchartType.Data)
            {
                var flowchartDataProp = property.FindPropertyRelative("flowchartData");
                EditorGUI.PropertyField(position, flowchartDataProp, new GUIContent("FlowchartData"));
            }
            position.y += GetHeight();

            var commandIndexProp = property.FindPropertyRelative("commandIndex");
            EditorGUI.PropertyField(position, commandIndexProp, new GUIContent("CommandIndex"));
            position.y += GetHeight();

            var isAwaitNestProp = property.FindPropertyRelative("isAwaitNest");
            EditorGUI.PropertyField(position, isAwaitNestProp, new GUIContent("IsAwaitNest"));

            EditorGUI.EndProperty();
        }
    }
}
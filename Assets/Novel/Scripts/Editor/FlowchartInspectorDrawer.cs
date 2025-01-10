using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(Flowchart))]
    public class FlowchartInspectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, 70),
                property.FindPropertyRelative("description"));
            position.y += EditorGUIUtility.singleLineHeight + 60;

            if (GUI.Button(new Rect(position.x, position.y, position.width, 30), "Open Flowchart Editor"))
            {
                FlowchartEditorWindow.OpenEditorWindow();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 100;
        }
    }
}
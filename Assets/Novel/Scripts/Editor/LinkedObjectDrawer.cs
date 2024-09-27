using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(Enum2ObjectListDataBase<,>.LinkedObject))]
    public class LinkedObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.targetObject is IEnum2ObjectListData data)
            {
                data.SetEnums();
            }

            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("type");
            EditorGUI.LabelField(
                new Rect(position.x, position.y - EditorGUIUtility.singleLineHeight / 2f, position.width, position.height),
                "Type");
            EditorGUI.LabelField(
                new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width, position.height),
                typeProp.enumDisplayNames[typeProp.enumValueIndex],
                new GUIStyle
                {
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState { textColor = Color.white }
                });
            position.y += EditorGUIUtility.singleLineHeight;

            var prefabProp = property.FindPropertyRelative("prefab");
            EditorGUI.PropertyField(
                new Rect(position.x, position.y, position.width, position.height / 2f),
                prefabProp, new GUIContent("Prefab"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }
    }
}
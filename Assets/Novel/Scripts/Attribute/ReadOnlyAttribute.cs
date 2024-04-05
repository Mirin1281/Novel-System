using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Novel
{
    [AttributeUsage(AttributeTargets.Field)]
    class ReadOnlyAttribute : PropertyAttribute { }
}

#if UNITY_EDITOR
namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif
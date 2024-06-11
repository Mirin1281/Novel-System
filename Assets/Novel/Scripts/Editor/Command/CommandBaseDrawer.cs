using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(CommandBase), useForChildren: false)]
    public class CommandBaseDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        { }

        protected float GetHeight(float? height = null) => height ?? EditorGUIUtility.singleLineHeight;
    }
}
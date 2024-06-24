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

        /// <summary>
        /// フィールドを描画します
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="fieldName">フィールドの名前</param>
        /// <returns>フィールドのプロパティ</returns>
        protected SerializedProperty DrawField(ref Rect position, SerializedProperty property, string fieldName)
        {
            var prop = property.FindPropertyRelative(fieldName);
            EditorGUI.PropertyField(position, prop, new GUIContent(prop.displayName));
            position.y += GetHeight();
            return prop;
        }

        protected float GetHeight(float? height = null) => height ?? EditorGUIUtility.singleLineHeight;
    }
}
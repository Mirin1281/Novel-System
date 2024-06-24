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
        /// �t�B�[���h��`�悵�܂�
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="fieldName">�t�B�[���h�̖��O</param>
        /// <returns>�t�B�[���h�̃v���p�e�B</returns>
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
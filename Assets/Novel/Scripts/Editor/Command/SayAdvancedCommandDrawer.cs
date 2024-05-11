using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(SayAdvancedCommand))]
    public class SayAdvancedCommandDrawer : SayCommandDrawer
    {
        /// <summary>
        /// Say�̒ǉ��v�f��`�悵�܂�(�Ԃ�l��Rect.y)
        /// </summary>
        protected override float DrawExtraContents(Rect position, SerializedProperty property, GUIContent label)
        {
            // �{�b�N�X�t�F�[�h���Ԃ̐ݒ� //
            var boxShowTimeProp = property.FindPropertyRelative("boxShowTime");
            EditorGUI.PropertyField(position, boxShowTimeProp, new GUIContent("BoxShowTime"));
            position.y += GetHeight();

            // �t���O�L�[�̐ݒ� //
            var flagKeysProp = property.FindPropertyRelative("flagKeys");
            EditorGUI.PropertyField(position, flagKeysProp, new GUIContent("FlagKeys"));
            position.y += GetArrayHeight(flagKeysProp);

            // �L�����N�^�[���̐ݒ� //
            var characterNameProp = property.FindPropertyRelative("characterName");
            EditorGUI.PropertyField(position, characterNameProp, new GUIContent("CharacterName"));
            position.y += GetHeight();

            return position.y;
        }

        float GetArrayHeight(SerializedProperty property)
        {
            if (property.isArray == false)
            {
                Debug.LogWarning("�v���p�e�B���z��ł͂���܂���I");
                return EditorGUIUtility.singleLineHeight;
            }
            if (property.isExpanded == false)
            {
                return EditorGUIUtility.singleLineHeight * 1.5f;
            }
            int length = property.arraySize;
            if (length is 0 or 1)
            {
                return EditorGUIUtility.singleLineHeight * 4f;
            }
            else
            {
                return (length + 3) * EditorGUIUtility.singleLineHeight;
            }
        }
    }
}
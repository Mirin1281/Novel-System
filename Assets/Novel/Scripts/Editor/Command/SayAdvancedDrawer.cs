using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(SayAdvanced))]
    public class SayAdvancedDrawer : SayDrawer
    {
        string overrideName = string.Empty;

        /// <summary>
        /// Say�̒ǉ��v�f��`�悵�܂�(�Ԃ�l��Rect.y)
        /// </summary>
        protected override (float posY, int arraySize) DrawExtraContents(Rect position, SerializedProperty property, GUIContent label)
        {
            // �{�b�N�X�^�C�v�̐ݒ� //
            DrawField(ref position, property, "boxType");

            // �{�b�N�X�t�F�[�h���Ԃ̐ݒ� //
            DrawField(ref position, property, "boxShowTime");

            // �L�����N�^�[���̐ݒ� //
            var characterNameProp = DrawField(ref position, property, "characterName");
            overrideName = characterNameProp.stringValue;

            // �t���O�L�[�̐ݒ� //
            var flagKeysProp = property.FindPropertyRelative("flagKeys");
            EditorGUI.PropertyField(position, flagKeysProp, new GUIContent(flagKeysProp.displayName));
            position.y += GetArrayHeight(flagKeysProp);

            return (position.y + flagKeysProp.arraySize, flagKeysProp.arraySize);
        }

        protected override (Color, string) GetCharacterStatus(CharacterData chara)
        {
            var (baseColor, baseName) = base.GetCharacterStatus(chara);
            var nameText = string.IsNullOrEmpty(overrideName) ? baseName : overrideName;
            return (baseColor, nameText);
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
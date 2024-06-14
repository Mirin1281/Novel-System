using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    /// <summary>
    /// �����͕ϐ����ɂ��Ă�������(nameof����)
    /// </summary>
    [CustomPropertyDrawer(typeof(DropDownCharacterAttribute))]
    public class DropDownCharacterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            DropDownCharacterAttribute commentAttribute = attribute as DropDownCharacterAttribute;
            CommandDrawerUtility.DropDownCharacterList(rect, property, commentAttribute.FieldName);
        }
    }
}

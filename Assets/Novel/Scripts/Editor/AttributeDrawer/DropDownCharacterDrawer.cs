using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    /// <summary>
    /// CharacterDataにつけるとドロップダウン方式で選択できます
    /// </summary>
    [CustomPropertyDrawer(typeof(DropDownCharacterAttribute))]
    public class DropDownCharacterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            CommandDrawerUtility.DropDownCharacterList(rect, property);
        }
    }
}

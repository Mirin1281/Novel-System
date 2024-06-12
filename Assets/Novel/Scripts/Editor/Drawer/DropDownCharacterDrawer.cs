using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    /// <summary>
    /// ˆø”‚Í•Ï”–¼‚É‚µ‚Ä‚­‚¾‚³‚¢(nameof„§)
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

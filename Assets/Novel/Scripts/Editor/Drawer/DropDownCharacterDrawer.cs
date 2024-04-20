using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(DropDownCharacterAttribute))]
    public class DropDownCharacterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            FlowchartEditorUtility.DropDownCharacterList(rect, property);
        }
    }
}

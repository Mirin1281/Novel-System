using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(SayAdvancedCommand))]
    public class SayAdvancedCommandDrawer : SayCommandDrawer
    {
        /// <summary>
        /// Sayの追加要素を描画します(返り値はRect.y)
        /// </summary>
        protected override float DrawExtraContents(Rect position, SerializedProperty property, GUIContent label)
        {
            // ボックスフェード時間の設定 //
            var boxShowTimeProp = property.FindPropertyRelative("boxShowTime");
            EditorGUI.PropertyField(position, boxShowTimeProp, new GUIContent("BoxShowTime"));
            position.y += GetHeight();

            // フラグキーの設定 //
            var flagKeysProp = property.FindPropertyRelative("flagKeys");
            EditorGUI.PropertyField(position, flagKeysProp, new GUIContent("FlagKeys"));
            position.y += GetArrayHeight(flagKeysProp);

            // キャラクター名の設定 //
            var characterNameProp = property.FindPropertyRelative("characterName");
            EditorGUI.PropertyField(position, characterNameProp, new GUIContent("CharacterName"));
            position.y += GetHeight();

            return position.y;
        }

        float GetArrayHeight(SerializedProperty property)
        {
            if (property.isArray == false)
            {
                Debug.LogWarning("プロパティが配列ではありません！");
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
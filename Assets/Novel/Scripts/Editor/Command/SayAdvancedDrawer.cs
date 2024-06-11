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
        /// Sayの追加要素を描画します(返り値はRect.y)
        /// </summary>
        protected override float DrawExtraContents(Rect position, SerializedProperty property, GUIContent label)
        {
            // ボックスタイプの設定 //
            var boxTypeProp = property.FindPropertyRelative("boxType");
            EditorGUI.PropertyField(position, boxTypeProp, new GUIContent("BoxType"));
            position.y += GetHeight();

            // ボックスフェード時間の設定 //
            var boxShowTimeProp = property.FindPropertyRelative("boxShowTime");
            EditorGUI.PropertyField(position, boxShowTimeProp, new GUIContent("BoxShowTime"));
            position.y += GetHeight();

            // キャラクター名の設定 //
            var characterNameProp = property.FindPropertyRelative("characterName");
            EditorGUI.PropertyField(position, characterNameProp, new GUIContent("CharacterName"));
            position.y += GetHeight();
            overrideName = characterNameProp.stringValue;

            // フラグキーの設定 //
            var flagKeysProp = property.FindPropertyRelative("flagKeys");
            EditorGUI.PropertyField(position, flagKeysProp, new GUIContent("FlagKeys"));
            position.y += GetArrayHeight(flagKeysProp);

            return position.y;
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
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
        protected override (float posY, int arraySize) DrawExtraContents(Rect position, SerializedProperty property, GUIContent label)
        {
            // ボックスタイプの設定 //
            DrawField(ref position, property, "boxType");

            // ボックスフェード時間の設定 //
            DrawField(ref position, property, "boxShowTime");

            // キャラクター名の設定 //
            var characterNameProp = DrawField(ref position, property, "characterName");
            overrideName = characterNameProp.stringValue;

            // フラグキーの設定 //
            var flagKeysProp = DrawField(ref position, property, "flagKeys");

            return (position.y + flagKeysProp.arraySize, flagKeysProp.arraySize);
        }

        protected override (Color, string) GetCharacterStatus(CharacterData chara)
        {
            var (baseColor, baseName) = base.GetCharacterStatus(chara);
            var nameText = string.IsNullOrEmpty(overrideName) ? baseName : overrideName;
            return (baseColor, nameText);
        }
    }
}
using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(Say))]
    public class SayDrawer : CommandBaseDrawer
    {
        const int storyTextArea = 70;
        const int buttonWidth = 30;
        const int buttonHeight = 30;

        /// <summary>
        /// SayAdvancedDrawerでoverrideする
        /// </summary>
        protected virtual (float posY, int arraySize) DrawExtraContents(Rect position, SerializedProperty property, GUIContent label)
            => (position.y, 0);

        protected virtual (Color, string) GetCharacterStatus(CharacterData chara)
        {
            var nameColor = chara == null ? Color.white : chara.NameColor;
            var nameText = chara == null ? null : chara.NameIncludeRuby;
            return (nameColor, nameText);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += GetHeight(10);
            EditorGUI.BeginProperty(position, label, property);

            // キャラクターの設定 //
            var characterProp = property.FindPropertyRelative("character");
            CharacterData chara = CommandDrawerUtility.DropDownCharacterList(position, characterProp);
            position.y += GetHeight();

            if (chara != null)
            {
                // 立ち絵の設定 //
                var spriteProp = property.FindPropertyRelative("changeSprite");
                CommandDrawerUtility.DropDownSpriteList(position, spriteProp, chara);
                position.y += GetHeight();
            }

            // テキストの設定 //
            var storyTextProp = property.FindPropertyRelative("storyText");
            EditorGUI.PropertyField(new Rect(
                position.x, position.y, position.width, storyTextArea),
                storyTextProp, new GUIContent(storyTextProp.displayName));
            position.y += GetHeight(storyTextArea);

            int arraySize;
            (position.y, arraySize) = DrawExtraContents(position, property, label);

            GUILayoutUtility.GetRect(0, 300 + arraySize * EditorGUIUtility.singleLineHeight);

            DrawHelp(position, storyTextProp, chara);
            EditorGUI.EndProperty();
        }

        void DrawHelp(Rect position, SerializedProperty textProp, CharacterData chara)
        {
            position.y += GetHeight(10);

            if (GUI.Button(new Rect(position.x + buttonWidth, position.y, position.width - buttonWidth * 2, buttonHeight),
                "テキストをプレビュー"))
            {
                var text = textProp.stringValue;
                var boxes = GameObject.FindObjectsByType<MessageBox>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                var boxType = chara == null ? Say.DefaultType : chara.BoxType;
                if (boxes == null || boxes.Length == 0)
                {
                    Debug.LogWarning(
                        "メッセージボックスの取得に失敗しました\n" +
                        "ヒエラルキー内にメッセージボックスを置いてください");
                }
                else
                {
                    bool isMatch = false;
                    foreach (var box in boxes)
                    {
                        if (box.IsTypeEqual(boxType))
                        {
                            var (nameColor, nameText) = GetCharacterStatus(chara);
                            box.Writer.PreviewText(nameColor, nameText, text);
                            isMatch = true;
                        }
                    }
                    if (isMatch == false)
                    {
                        Debug.LogWarning("メッセージボックスとキャラクターのタイプが合いませんでした");
                        var (nameColor, nameText) = GetCharacterStatus(chara);
                        boxes[0].Writer.PreviewText(nameColor, nameText, text);
                    }
                }
            }
            position.y += GetHeight(buttonHeight);

            var tooltipText =
                "【タグについて】\n" +
                "<???>の構文を用いることで、リッチテキストや独自の修飾タグを使うことができます\n\n" +

                "◆\"<r=さだめ>運命</r>\"\n" +
                "ルビを振ります\n" +
                "注意:他のタグと組み合わせる場合は入れたいテキストの内側に入れてください\n\n" +

                "◆\"<s=2.5>オタクの早口</s>\"\n" +
                "表示スピードを数値倍します\n\n" +

                "◆\"<w=1>\"\n" +
                "数値の秒数だけ待機します(入力時はスキップ)\n\n" +

                "◆\"<wi>\"\n" +
                "入力があるまで待機します\n\n" +

                "◆\"<flag0>万円が落ちていた\"\n" +
                $"フラグの値を表示します({nameof(SayAdvanced)}限定)\n" +
                "FlagKeysの配列の要素と対応しています\n";

            EditorGUI.LabelField(new Rect(
                position.x + buttonWidth / 2f, position.y, position.width, position.height),
                new GUIContent("◆タグについて(ホバーで表示)", tooltipText));
        }
    }
}
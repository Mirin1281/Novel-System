using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(SayCommand))]
    public class SayCommandDrawer : PropertyDrawer
    {
        const int storyTextArea = 70;
        const int buttonHeight = 30;

        /// <summary>
        /// SayAdvancedCommandDrawerでoverrideする
        /// </summary>
        protected virtual float DrawExtraContents(Rect position, SerializedProperty property, GUIContent label)
            => position.y;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += GetHeight(10);

            CharacterData chara = FlowchartEditorUtility.DropDownCharacterList(position, property);
            position.y += GetHeight();

            if (chara != null && chara.Portraits != null && chara.Portraits.Count() != 0)
            {
                // 立ち絵の設定 //
                var portraitProp = property.FindPropertyRelative("changeSprite");
                var portraitsArray = chara.Portraits.Prepend(null).ToArray();
                int previousPortraitIndex = Array.IndexOf(
                    portraitsArray, portraitProp.objectReferenceValue as Sprite);
                int selectedPortraitIndex = EditorGUI.Popup(position, "ChangeSprite", previousPortraitIndex,
                    portraitsArray.Select(p => p == null ? "<Null>" : p.name).ToArray());

                if (selectedPortraitIndex != previousPortraitIndex)
                {
                    portraitProp.objectReferenceValue = portraitsArray[selectedPortraitIndex];
                    portraitProp.serializedObject.ApplyModifiedProperties();
                }
                position.y += GetHeight();
            }
            // テキストの設定 //
            var storyTextProp = property.FindPropertyRelative("storyText");
            EditorGUI.PropertyField(GetRect(position, storyTextArea),
                storyTextProp, new GUIContent("StoryText"));
            position.y += GetHeight(storyTextArea);

            position.y = DrawExtraContents(position, property, label);

            DrawHelp(position, storyTextProp, chara);
        }

        protected void DrawHelp(Rect position, SerializedProperty textProp, CharacterData chara)
        {
            position.y += GetHeight(10);
            if (GUI.Button(GetRect(position, buttonHeight), "テキストをプレビュー"))
            {
                var text = textProp.stringValue;
                var boxes = GameObject.FindObjectsByType<MessageBox>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                if (boxes == null || boxes.Length == 0)
                {
                    Debug.LogWarning("メッセージボックスがありません！");
                }
                else
                {
                    bool isMatch = false;
                    foreach (var box in boxes)
                    {
                        if (box.IsMeetType(chara.BoxType))
                        {
                            box.Writer.PreviewText(chara, text);
                            isMatch = true;
                        }
                    }
                    if (isMatch == false)
                    {
                        Debug.LogWarning("ボックスとキャラクタのタイプが合いませんでした");
                        boxes[0].Writer.PreviewText(chara, text);
                    }
                }
            }
            position.y += GetHeight(buttonHeight);

            var tooltipText =
                "【タグについて】\n" +
                "<hoge>の構文を用いることで、リッチテキストや独自の修飾タグを使うことができます\n\n" +

                "◆<r=さだめ>運命</r>\n" +
                "　ルビを振ります\n" +
                "　注意点: 他と組み合わせる場合は内側に入れてください\n\n" +

                "◆<flag0>万円が落ちていた\n" +
                "　フラグの値を表示します\n" +
                "　FlagKeysの配列の要素と対応しています\n\n" +

                "◆<s=2.5>オタクの早口</s>\n" +
                "　表示スピードを数値倍します\n\n" +

                "◆<w=1>\n" +
                "　数値の秒数だけ待機します(入力時はスキップ)\n\n" +

                "◆<wi>\n" +
                "　入力があるまで待機します\n\n" +

                "◆<wic>\n" +
                "　入力があるまで待機し、\n" +
                "　それまでの文を消してから次を表示します\n";
            EditorGUI.LabelField(position, new GUIContent("◆タグについて(ホバーで表示)", tooltipText));
        }

        protected float GetHeight(float? height = null) => height ?? EditorGUIUtility.singleLineHeight;

        Rect GetRect(Rect rect, float? height = null) =>
            new Rect(
                rect.x, rect.y,
                rect.width, height ?? EditorGUIUtility.singleLineHeight);
    }
}
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
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += GetHeight(10);
            EditorGUI.BeginProperty(position, label, property);
            
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
            EditorGUI.PropertyField(GetRect(position, 70),
                storyTextProp, new GUIContent("StoryText"));
            position.y += GetHeight(70);

            // ボックスフェード時間の設定 //
            var boxShowTimeProp = property.FindPropertyRelative("boxShowTime");
            EditorGUI.PropertyField(position, boxShowTimeProp, new GUIContent("BoxShowTime"));
            position.y += GetHeight();

            // フラグキーの設定 //
            var flagKeysProp = property.FindPropertyRelative("flagKeys");
            EditorGUI.PropertyField(position, flagKeysProp, new GUIContent("FlagKeys"));
            position.y += GetArrayHeight(flagKeysProp);

            GUILayout.Space(10);

            if (GUI.Button(GetRect(position, 30), "テキストをプレビュー"))
            {
                var text = storyTextProp.stringValue;
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
            position.y += GetHeight(30);

            var tooltipText =
                "【タグについて】\n" +
                "<hoge>の構文を用いることで、リッチテキストや独自の修飾タグを使うことができます\n\n" +

                "◆<r=さだめ>運命</r>\n" +
                "　ルビを振ります\n" +
                "　注意点: 他と組み合わせる場合は内側に入れてください\n\n" +

                "◆<flag0>万円が落ちていた\n" +
                "　フラグの値を表示します\n" +
                "　FlagKeysの配列の要素と対応しています\n\n" +

                "◆<sp=2.5>オタクの早口</s>\n" +
                "　表示スピードを数値倍します\n\n" +

                "◆<w=1>\n" +
                "　数値の秒数だけ待機します(入力時はスキップ)\n\n" +

                "◆<wi>\n" +
                "　入力があるまで待機します\n\n" +

                "◆<wic>\n" +
                "　入力があるまで待機し、\n" +
                "　それまでの文を消してから次を表示します\n";
            EditorGUI.LabelField(position, new GUIContent("◆タグについて(ホバーで表示)", tooltipText));
            EditorGUI.EndProperty();
        }

        float GetHeight(float? height = null) => height ?? EditorGUIUtility.singleLineHeight;

        float GetArrayHeight(SerializedProperty property)
        {
            if(property.isArray == false)
            {
                Debug.LogWarning("プロパティが配列ではありません！");
                return EditorGUIUtility.singleLineHeight;
            }
            if(property.isExpanded == false)
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

        Rect GetRect(Rect rect, float? height = null) =>
            new Rect(
                rect.x, rect.y,
                rect.width, height ?? EditorGUIUtility.singleLineHeight);
    }
}
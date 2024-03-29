using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Novel.Command
{
    [CustomPropertyDrawer(typeof(SayCommand))]
    public class SayCommandDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUILayout.Space(-10);

            var style = new GUIStyle();
            style.normal.background = Texture2D.whiteTexture;

            var c = GUI.color;
            GUI.color = new Color(0.08f, 0.2f, 0.2f, 0.3f); // 水色
            EditorGUILayout.BeginVertical(style);
            GUI.color = c;

            // キャラクターの設定 //
            var charaProp = property.FindPropertyRelative("character");

            // characterFolderPath内のキャラクターデータを取得
            var characterArray = AssetDatabase.FindAssets(
                "t:ScriptableObject", new string[] { NameContainer.CHARACTER_PATH })
                .Select(c => AssetDatabase.GUIDToAssetPath(c))
                .Select(c => AssetDatabase.LoadAssetAtPath<CharacterData>(c))
                .Prepend(null)
                .ToArray();

            int previousCharaIndex = Array.IndexOf(
                characterArray, charaProp.objectReferenceValue as CharacterData);
            int selectedCharaIndex = EditorGUILayout.Popup("Character", previousCharaIndex,
                characterArray.Select(c => c == null ? "<Null>" : c.CharacterName).ToArray());
            var chara = characterArray[selectedCharaIndex];

            if (selectedCharaIndex != previousCharaIndex)
            {
                charaProp.objectReferenceValue = chara;
                charaProp.serializedObject.ApplyModifiedProperties();
            }

            if (chara != null && chara.Portraits != null && chara.Portraits.Count() != 0)
            {
                // 立ち絵の設定 //
                var portraitProp = property.FindPropertyRelative("changeSprite");
                var portraitsArray = chara.Portraits.Prepend(null).ToArray();
                int previousPortraitIndex = Array.IndexOf(
                    portraitsArray, portraitProp.objectReferenceValue as Sprite);
                int selectedPortraitIndex = EditorGUILayout.Popup("changeSprite", previousPortraitIndex,
                    portraitsArray.Select(p => p == null ? "<Null>" : p.name).ToArray());

                if (selectedPortraitIndex != previousPortraitIndex)
                {
                    portraitProp.objectReferenceValue = portraitsArray[selectedPortraitIndex];
                    portraitProp.serializedObject.ApplyModifiedProperties();
                }
            }

            // テキストの設定 //
            var storyTextProp = property.FindPropertyRelative("storyText");
            EditorGUILayout.PropertyField(storyTextProp, new GUIContent("StoryText"),
                new GUILayoutOption[] { GUILayout.Height(70) });

            // ボックスフェード時間の設定 //
            var boxShowTimeProp = property.FindPropertyRelative("boxShowTime");
            EditorGUILayout.PropertyField(boxShowTimeProp, new GUIContent("BoxShowTime"));

            // フラグキーの設定 //
            var flagKeysProp = property.FindPropertyRelative("flagKeys");
            EditorGUILayout.PropertyField(flagKeysProp, new GUIContent("FlagKeys"));

            EditorGUILayout.EndVertical();
        }
    }
}

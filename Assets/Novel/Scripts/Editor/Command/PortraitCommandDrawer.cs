using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Novel.Command;

namespace Novel.Editor
{
    using ActionType = PortraitCommand.ActionType;

    [CustomPropertyDrawer(typeof(PortraitCommand))]
    public class PortraitCommandDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUILayout.Space(-10);

            // キャラクターの設定 //
            var charaProp = property.FindPropertyRelative("character");

            // 特定のフォルダ内のキャラクターデータを取得
            var characterArray = FlowchartEditorUtility.GetAllScriptableObjects<CharacterData>()
                .Prepend(null).ToArray();

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

            // アクションの設定 //
            var actionTypeProp = property.FindPropertyRelative("actionType");
            EditorGUILayout.PropertyField(actionTypeProp, new GUIContent("ActionType"));

            if((chara != null && chara.Portraits != null && chara.Portraits.Count() != 0) &&
                (ActionType)actionTypeProp.enumValueIndex == ActionType.Show ||
                (ActionType)actionTypeProp.enumValueIndex == ActionType.Change)
            {
                // 立ち絵の設定 //
                var portraitSpriteProp = property.FindPropertyRelative("portraitSprite");
                var portraitsArray = chara.Portraits.Prepend(null).ToArray();
                int previousPortraitIndex = Array.IndexOf(
                    portraitsArray, portraitSpriteProp.objectReferenceValue as Sprite);
                int selectedPortraitIndex = EditorGUILayout.Popup("PortraitSprite", previousPortraitIndex,
                    portraitsArray.Select(p => p == null ? "<Null>" : p.name).ToArray());

                if (selectedPortraitIndex != previousPortraitIndex)
                {
                    portraitSpriteProp.objectReferenceValue = portraitsArray[selectedPortraitIndex];
                    portraitSpriteProp.serializedObject.ApplyModifiedProperties();
                }
            }

            if ((ActionType)actionTypeProp.enumValueIndex == ActionType.Show)
            {
                // ポジションの設定 //
                var positionTypeProp = property.FindPropertyRelative("positionType");
                EditorGUILayout.PropertyField(positionTypeProp, new GUIContent("PositionType"));

                if ((PortraitPosType)positionTypeProp.enumValueIndex == PortraitPosType.Custom)
                {
                    // 上書きポジションの設定 //
                    var overridePosProp = property.FindPropertyRelative("overridePos");
                    EditorGUILayout.PropertyField(overridePosProp, new GUIContent("OverridePos"));
                }
            }

            if((ActionType)actionTypeProp.enumValueIndex == ActionType.Show ||
                (ActionType)actionTypeProp.enumValueIndex == ActionType.Clear)
            {
                // フェード時間の設定 //
                var fadeTimeProp = property.FindPropertyRelative("fadeTime");
                EditorGUILayout.PropertyField(fadeTimeProp, new GUIContent("FadeTime"));

                // 待機するかの設定 //
                var isAwaitProp = property.FindPropertyRelative("isAwait");
                EditorGUILayout.PropertyField(isAwaitProp, new GUIContent("IsAwait"));
            }
        }
    }
}

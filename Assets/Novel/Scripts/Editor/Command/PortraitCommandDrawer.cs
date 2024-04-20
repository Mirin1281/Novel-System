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
            position.y += GetHeight(10);
            EditorGUI.BeginProperty(position, label, property);

            CharacterData chara = FlowchartEditorUtility.DropDownCharacterList(position, property);
            position.y += GetHeight();

            // アクションの設定 //
            var actionTypeProp = property.FindPropertyRelative("actionType");
            EditorGUI.PropertyField(position, actionTypeProp, new GUIContent("ActionType"));
            position.y += GetHeight();

            if ((chara != null && chara.Portraits != null && chara.Portraits.Count() != 0) &&
                (ActionType)actionTypeProp.enumValueIndex == ActionType.Show ||
                (ActionType)actionTypeProp.enumValueIndex == ActionType.Change)
            {
                // 立ち絵の設定 //
                var portraitSpriteProp = property.FindPropertyRelative("portraitSprite");
                var portraitsArray = chara.Portraits.Prepend(null).ToArray();
                int previousPortraitIndex = Array.IndexOf(
                    portraitsArray, portraitSpriteProp.objectReferenceValue as Sprite);
                int selectedPortraitIndex = EditorGUI.Popup(position, "PortraitSprite", previousPortraitIndex,
                    portraitsArray.Select(p => p == null ? "<Null>" : p.name).ToArray());

                if (selectedPortraitIndex != previousPortraitIndex)
                {
                    portraitSpriteProp.objectReferenceValue = portraitsArray[selectedPortraitIndex];
                    portraitSpriteProp.serializedObject.ApplyModifiedProperties();
                }
                position.y += GetHeight();
            }

            if ((ActionType)actionTypeProp.enumValueIndex == ActionType.Show)
            {
                // ポジションの設定 //
                var positionTypeProp = property.FindPropertyRelative("positionType");
                EditorGUI.PropertyField(position, positionTypeProp, new GUIContent("PositionType"));

                if ((PortraitPosType)positionTypeProp.enumValueIndex == PortraitPosType.Custom)
                {
                    // 上書きポジションの設定 //
                    var overridePosProp = property.FindPropertyRelative("overridePos");
                    EditorGUI.PropertyField(position, overridePosProp, new GUIContent("OverridePos"));
                    position.y += GetHeight();
                }
                position.y += GetHeight();
            }

            if ((ActionType)actionTypeProp.enumValueIndex == ActionType.Show ||
                (ActionType)actionTypeProp.enumValueIndex == ActionType.Clear)
            {
                // フェード時間の設定 //
                var fadeTimeProp = property.FindPropertyRelative("fadeTime");
                EditorGUI.PropertyField(position, fadeTimeProp, new GUIContent("FadeTime"));
                position.y += GetHeight();

                // 待機するかの設定 //
                var isAwaitProp = property.FindPropertyRelative("isAwait");
                EditorGUI.PropertyField(position, isAwaitProp, new GUIContent("IsAwait"));
            }
            EditorGUI.EndProperty();
        }

        float GetHeight(float? height = null) => height ?? EditorGUIUtility.singleLineHeight;
    }
}

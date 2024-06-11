using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Novel.Command;

namespace Novel.Editor
{
    using ActionType = Command.Portrait.ActionType;

    [CustomPropertyDrawer(typeof(Command.Portrait))]
    public class PortraitDrawer : CommandBaseDrawer
    {
        static readonly int previewXOffset = 0;
        static readonly int previewHeightOffset = 20;
        static readonly int previewSize = 300;

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
            ActionType actionType = (ActionType)actionTypeProp.enumValueIndex;

            SerializedProperty portraitSpriteProp = null;

            if ((chara != null && chara.Portraits != null && chara.Portraits.Count() != 0) &&
                (actionType == ActionType.Show || actionType == ActionType.Change))
            {
                // 立ち絵の設定 //
                portraitSpriteProp = property.FindPropertyRelative("portraitSprite");
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

            if (actionType == ActionType.Show)
            {
                // ポジションの設定 //
                var positionTypeProp = property.FindPropertyRelative("positionType");
                EditorGUI.PropertyField(position, positionTypeProp, new GUIContent("PositionType"));
                position.y += GetHeight();

                if ((PortraitPositionType)positionTypeProp.enumValueIndex == PortraitPositionType.Custom)
                {
                    // 上書きポジションの設定 //
                    var overridePosProp = property.FindPropertyRelative("overridePos");
                    EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), new GUIContent("OverridePos"));
                    overridePosProp.vector2Value = EditorGUI.Vector2Field(
                        new Rect(position.x + EditorGUIUtility.labelWidth - 10f, position.y,
                            position.width - 140, position.height),
                        string.Empty, overridePosProp.vector2Value);
                    position.y += GetHeight();
                }                
            }

            if (actionType == ActionType.Show || actionType == ActionType.Clear)
            {
                // フェード時間の設定 //
                var fadeTimeProp = property.FindPropertyRelative("fadeTime");
                EditorGUI.PropertyField(position, fadeTimeProp, new GUIContent("FadeTime"));
                position.y += GetHeight();

                // 待機するかの設定 //
                var isAwaitProp = property.FindPropertyRelative("isAwait");
                EditorGUI.PropertyField(position, isAwaitProp, new GUIContent("IsAwait"));
                position.y += GetHeight();
            }

            if (actionType == ActionType.Show || actionType == ActionType.Change)
            {
                // 立ち絵のプレビュー //
                Sprite sprite = portraitSpriteProp?.objectReferenceValue as Sprite;
                if(sprite != null && sprite.texture != null)
                {
                    EditorGUI.LabelField(
                        new Rect(
                            position.width / 2f - previewSize / 3f + previewXOffset,
                            position.y - previewHeightOffset,
                            previewSize, previewSize),
                        new GUIContent(sprite.texture));
                }
            }

            EditorGUI.EndProperty();
        }
    }
}

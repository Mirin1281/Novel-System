using UnityEngine;
using UnityEditor;
using PortraitPositionType = Novel.Portrait.PortraitPositionType;

namespace Novel.Editor
{
    using ActionType = Command.Portrait.ActionType;

    [CustomPropertyDrawer(typeof(Command.Portrait))]
    public class PortraitDrawer : CommandBaseDrawer
    {
        static readonly int previewXOffset = 0;
        static readonly int previewHeightOffset = 0;
        static readonly int previewSize = 300;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += GetHeight(10);

            // キャラクターの設定 //
            var characterProp = property.FindPropertyRelative("character");
            CharacterData chara = CommandDrawerUtility.DropDownCharacterList(position, characterProp);
            position.y += GetHeight();

            EditorGUI.BeginDisabledGroup(chara == null);

            // アクションの設定 //
            var actionTypeProp = DrawField(ref position, property, "actionType");
            var actionType = (ActionType)actionTypeProp.enumValueIndex;

            // 立ち絵の設定 //
            Sprite sprite = null;
            if(actionType == ActionType.Show || actionType == ActionType.Change)
            {
                var spriteProp = property.FindPropertyRelative("portraitSprite");
                sprite = CommandDrawerUtility.DropDownSpriteList(position, spriteProp, chara);
                position.y += GetHeight();
            }

            if (actionType == ActionType.Show)
            {
                // ポジションの設定 //
                var positionTypeProp = DrawField(ref position, property, "positionType");
                var positionType = (PortraitPositionType)positionTypeProp.enumValueIndex;

                if (positionType == PortraitPositionType.Custom)
                {
                    // 上書きポジションの設定 //
                    var overridePosProp = property.FindPropertyRelative("overridePos");
                    EditorGUI.LabelField(
                        new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height),
                        new GUIContent(overridePosProp.displayName));
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
                DrawField(ref position, property, "fadeTime");

                // 待機するかの設定 //
                DrawField(ref position, property, "isAwait");
            }

            if (actionType == ActionType.Show || actionType == ActionType.Change)
            {
                // 立ち絵のプレビュー //
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

            if(sprite != null)
            {
                GUILayoutUtility.GetRect(0, 250 + sprite.textureRect.height / 2f);
            }
            

            EditorGUI.EndDisabledGroup();
        }
    }
}

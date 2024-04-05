using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(PortraitTurnCommand))]
    public class PortraitTurnCommandDrawer : PropertyDrawer
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

            // 時間の設定 //
            var timeProp = property.FindPropertyRelative("time");
            EditorGUILayout.PropertyField(timeProp, new GUIContent("Time"));

            // 待機の設定 //
            var isAwaitProp = property.FindPropertyRelative("isAwait");
            EditorGUILayout.PropertyField(isAwaitProp, new GUIContent("IsAwait"));
        }
    }
}

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Novel.Command
{
    [CustomPropertyDrawer(typeof(PortraitTurnCommand))]
    public class PortraitTurnCommandDrawer : PropertyDrawer
    {
        const string characterFolderName = "Character";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUILayout.Space(-10);

            // キャラクターの設定 //
            var charaProp = property.FindPropertyRelative("character");

            // characterFolderPath内のキャラクターデータを取得
            var characterArray = AssetDatabase.FindAssets(
                "t:ScriptableObject", new string[] { NameContainer.RESOURCES_PATH + characterFolderName })
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

            // 時間の設定 //
            var timeProp = property.FindPropertyRelative("time");
            EditorGUILayout.PropertyField(timeProp, new GUIContent("Time"));

            // 待機の設定 //
            var isAwaitProp = property.FindPropertyRelative("isAwait");
            EditorGUILayout.PropertyField(isAwaitProp, new GUIContent("IsAwait"));
        }
    }
}

using Novel.Command;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novel.Editor
{
    public static class FlowchartEditorUtility
    {
        /// <summary>
        /// 絶対パスから Assets/のパスに変換する
        /// </summary>
        public static string AbsoluteToAssetsPath(string path)
        {
            return path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }


        public static void DestroyScritableObject(ScriptableObject obj)
        {
            var path = GetExistFolderPath(obj);
            var deleteName = obj.name;
            Object.DestroyImmediate(obj, true);
            File.Delete($"{path}/{deleteName}.asset");
            File.Delete($"{path}/{deleteName}.asset.meta");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        // プロジェクト上にあるアセットのパスを返します
        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(dataPath)) return null;
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        /// <summary>
        /// ファイル名を重複を考慮して付けます
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="name">名前(拡張子は除く)</param>
        /// <param name="extension">拡張子(.は除く)</param>
        /// <returns></returns>
        public static string GetFileName(string path, string name, string extension)
        {
            int i = 1;
            var targetName = name;
            while (File.Exists($"{path}/{targetName}.{extension}"))
            {
                targetName = $"{name}_({i++})";
            }
            return $"{targetName}.{extension}";
        }

        public static T[] GetAllScriptableObjects<T>(string folderName = null) where T : ScriptableObject
        {
            string[] guids = folderName == null
                ? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                : AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folderName });
            return guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
        }

        /// <summary>
        /// CommandDataを作成します
        /// </summary>
        /// <param name="path">生成するフォルダのパス</param>
        /// <param name="baseName">名前</param>
        /// <returns></returns>
        public static CommandData CreateCommandData(string path, string baseName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var name = GetFileName(path, baseName, "asset");
            var cmdData = ScriptableObject.CreateInstance<CommandData>();
            AssetDatabase.CreateAsset(cmdData, Path.Combine(path, name));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return cmdData;
        }
    }

    /// <summary>
    /// コマンドの表示のヘルパー
    /// </summary>
    public static class CommandDrawerUtility
    {
        /// <summary>
        /// CharacterDataのドロップダウンリストを表示します
        /// </summary>
        public static CharacterData DropDownCharacterList(Rect position, SerializedProperty property, string fieldName)
        {
            var characterProp = property.FindPropertyRelative(fieldName);
            var characterArray = FlowchartEditorUtility.GetAllScriptableObjects<CharacterData>()
                .Prepend(null).ToArray();
            int previousCharaIndex = Array.IndexOf(
                characterArray, characterProp.objectReferenceValue as CharacterData);
            int selectedCharaIndex = EditorGUI.Popup(position, fieldName, previousCharaIndex,
                characterArray.Select(c => c == null ? "<Null>" : c.CharacterName).ToArray());
            CharacterData chara = characterArray[selectedCharaIndex];

            if (previousCharaIndex != selectedCharaIndex)
            {
                characterProp.objectReferenceValue = chara;
                characterProp.serializedObject.ApplyModifiedProperties();
            }
            return chara;
        }

        /// <summary>
        /// CharacterData内のスプライトのドロップダウンリストを表示します
        /// </summary>
        public static Sprite DropDownSpriteList(Rect position, SerializedProperty property, CharacterData character, string fieldName)
        {
            if (!(character != null && character.Portraits != null && character.Portraits.Count() != 0)) return null;
            var portraitProp = property.FindPropertyRelative(fieldName);
            var portraitsArray = character.Portraits.Prepend(null).ToArray();
            int previousPortraitIndex = Array.IndexOf(
                portraitsArray, portraitProp.objectReferenceValue as Sprite);
            int selectedPortraitIndex = EditorGUI.Popup(position, fieldName, previousPortraitIndex,
                portraitsArray.Select(p => p == null ? "<Null>" : p.name).ToArray());
            Sprite sprite = null;

            if (selectedPortraitIndex != previousPortraitIndex)
            {
                sprite = portraitsArray[selectedPortraitIndex];
                portraitProp.objectReferenceValue = sprite;
                portraitProp.serializedObject.ApplyModifiedProperties();
            }
            return sprite;
        }
    }
}
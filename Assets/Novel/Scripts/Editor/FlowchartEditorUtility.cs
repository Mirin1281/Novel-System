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
        /// ��΃p�X���� Assets/�̃p�X�ɕϊ�����
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

        // �v���W�F�N�g��ɂ���A�Z�b�g�̃p�X��Ԃ��܂�
        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(dataPath)) return null;
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        /// <summary>
        /// �t�@�C�������d�����l�����ĕt���܂�
        /// </summary>
        /// <param name="path">�p�X</param>
        /// <param name="name">���O(�g���q�͏���)</param>
        /// <param name="extension">�g���q(.�͏���)</param>
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
        /// CommandData���쐬���܂�
        /// </summary>
        /// <param name="path">��������t�H���_�̃p�X</param>
        /// <param name="baseName">���O</param>
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

        /// <summary>
        /// (�J�X�^���G�f�B�^)CharacterData�̃h���b�v�_�E�����X�g��\�����܂�
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static CharacterData DropDownCharacterList(Rect position, SerializedProperty property)
        {
            var characterProp = property.FindPropertyRelative("character");
            var characterArray = GetAllScriptableObjects<CharacterData>()
                .Prepend(null).ToArray();

            if (characterProp == null)
            {
                characterProp = property;
            }
            int previousCharaIndex = Array.IndexOf(
                characterArray, characterProp.objectReferenceValue as CharacterData);
            int selectedCharaIndex = EditorGUI.Popup(position, "Character", previousCharaIndex,
                characterArray.Select(c => c == null ? "<Null>" : c.CharacterName).ToArray());
            var chara = characterArray[selectedCharaIndex];

            if (previousCharaIndex != selectedCharaIndex)
            {
                characterProp.objectReferenceValue = chara;
                characterProp.serializedObject.ApplyModifiedProperties();
            }
            return chara;
        }
    }
}
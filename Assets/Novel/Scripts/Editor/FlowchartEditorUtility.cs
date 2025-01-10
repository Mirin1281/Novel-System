using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novel.Editor
{
    public static class FlowchartEditorUtility
    {
        /// <summary>
        /// ��΃p�X���� Assets/�̃p�X�ɕϊ����܂�
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

        /// <summary>
        /// C#�X�N���v�g������p�X���������܂�
        /// </summary>
        public static bool TryGetScriptPath(string fileName, out string relativePath)
        {
            var assetName = fileName;
            var path = AssetDatabase.FindAssets(assetName + " t:Script")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(str => string.Equals(Path.GetFileNameWithoutExtension(str),
                    assetName, StringComparison.CurrentCultureIgnoreCase));

            bool isExist = !string.IsNullOrEmpty(path);
            if (isExist)
            {
                relativePath = AbsoluteToAssetsPath(path);
            }
            else
            {
                relativePath = string.Empty;
            }
            return isExist;
        }

        /// <summary>
        /// �v���W�F�N�g��ɂ���A�Z�b�g�̃p�X��Ԃ��܂�
        /// </summary>
        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(dataPath)) return null;
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        /// <summary>
        /// �w�肵�����O����ɁA�t�H���_���ŏd�����Ȃ����O�𐶐����܂�
        /// </summary>
        /// <param name="folderPath">�ΏۂƂȂ�t�H���_�̃p�X</param>
        /// <param name="baseName">���O(�g���q�͏���)</param>
        /// <param name="extension">�g���q(.�͏���)</param>
        /// <returns></returns>
        public static string GenerateAssetName(string folderPath, string baseName, string extension = null)
        {
            var paths = AssetDatabase.FindAssets(null, new string[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath);

            var existingNames = new HashSet<string>();
            foreach (var p in paths)
            {
                // �T�u�t�H���_�͌������Ȃ��Ă����̂ŏ��O
                if (AbsoluteToAssetsPath(Path.GetDirectoryName(p)) != folderPath.TrimEnd('/')) continue;
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(p);
                foreach (var o in objs)
                {
                    existingNames.Add(o.name);
                }
            }

            string trimedName = baseName;
            string regex = @" \( ?\d+\)$";
            if (Regex.IsMatch(baseName, regex))
            {
                trimedName = Regex.Replace(baseName, regex, string.Empty);
            }

            string exStr = string.IsNullOrEmpty(extension) ? string.Empty : $".{extension}";

            // �x�[�X�����d�����Ȃ��ꍇ�A���̂܂ܕԂ�
            if (!existingNames.Contains(trimedName))
            {
                return $"{trimedName}{exStr}";
            }

            // �d������ꍇ�A�u(n)�v��t���ă��j�[�N�Ȗ��O��T��
            int suffix = 1;
            while (true)
            {
                string newName = $"{trimedName} ({suffix})";
                if (!existingNames.Contains(newName))
                {
                    return $"{newName}{exStr}";
                }
                suffix++;
            }
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
        /// CommandData���쐬���AFlowchartData�̎q�ɐݒ肵�܂�
        /// </summary>
        /// <param name="parentData">�e�ƂȂ�FlowchartData</param>
        /// <param name="baseName">���O</param>
        /// <returns></returns>
        public static CommandData CreateSubCommandData(FlowchartData parentData, string baseName)
        {
            var cmdData = ScriptableObject.CreateInstance<CommandData>();
            string path = AssetDatabase.GetAssetPath(parentData);
            int lastIndex = path.LastIndexOf('/');
            string folderPath = path.Substring(0, lastIndex);
            cmdData.name = GenerateAssetName(folderPath, baseName);
            AssetDatabase.AddObjectToAsset(cmdData, parentData);
            Undo.RegisterCreatedObjectUndo(cmdData, "Create Command");
            AssetDatabase.SaveAssets();
            return cmdData;
        }

        public static CommandData DuplicateSubCommandData(FlowchartData parentData, CommandData cmdData)
        {
            var createdCmd = Object.Instantiate(cmdData);
            string path = AssetDatabase.GetAssetPath(parentData);
            int lastIndex = path.LastIndexOf('/');
            string folderPath = path.Substring(0, lastIndex);
            createdCmd.name = GenerateAssetName(folderPath, createdCmd.name);
            AssetDatabase.AddObjectToAsset(createdCmd, parentData);
            Undo.RegisterCreatedObjectUndo(createdCmd, "Duplicate Command");
            AssetDatabase.SaveAssets();
            return createdCmd;
        }

        /// <summary>
        /// �s�v��CommandData���폜���܂�
        /// </summary>
        public static void RemoveAllUnusedCommandData()
        {
            int removeCount = 0;
            // �ǂ̃t���[�`���[�g�ɂ������Ă��Ȃ�CommandData���폜
            var flowchartDatas = GetAllScriptableObjects<FlowchartData>();
            foreach (var d in GetAllScriptableObjects<CommandData>())
            {
                if (IsUsed(d, flowchartDatas) == false)
                {
                    removeCount++;
                    DestroyScritableObject(d);
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            if (removeCount > 0)
            {
                Debug.Log($"�s�v�ȃf�[�^��{removeCount}�폜����܂���");
            }
            else
            {
                Debug.Log($"�s�v�ȃf�[�^�̌������������܂����B�폜���ꂽ�f�[�^�͂���܂���");
            }


            static bool IsUsed(CommandData targetData, FlowchartData[] flowchartDatas)
            {
                foreach (var flowchartData in flowchartDatas)
                {
                    if (flowchartData.IsUsed(targetData)) return true;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// �R�}���h�\���̃��[�e�B���e�B
    /// </summary>
    public static class CommandDrawerUtility
    {
        /// <summary>
        /// CharacterData�̃h���b�v�_�E�����X�g��\�����܂�
        /// </summary>
        public static CharacterData DropDownCharacterList(Rect position, SerializedProperty property)
        {
            var characterArray = FlowchartEditorUtility.GetAllScriptableObjects<CharacterData>()
                .Prepend(null).ToArray();
            int previousCharaIndex = Array.IndexOf(
                    characterArray, property.objectReferenceValue as CharacterData);
            int selectedCharaIndex = EditorGUI.Popup(position, property.displayName, previousCharaIndex,
                characterArray.Select(c => c == null ? "<Null>" : c.CharacterName).ToArray());

            if (previousCharaIndex != selectedCharaIndex)
            {
                property.objectReferenceValue = characterArray[selectedCharaIndex];
                property.serializedObject.ApplyModifiedProperties();
            }

            if (selectedCharaIndex < characterArray.Length && selectedCharaIndex >= 0)
            {
                return characterArray[selectedCharaIndex];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// CharacterData���̃X�v���C�g�̃h���b�v�_�E�����X�g��\�����܂�
        /// </summary>
        public static Sprite DropDownSpriteList(Rect position, SerializedProperty property, CharacterData character)
        {
            if (character == null || character.Portraits == null || character.Portraits.Count() == 0) return null;
            Sprite[] portraitsArray = character.Portraits.Prepend(null).ToArray();
            int previousPortraitIndex = Array.IndexOf(portraitsArray, property.objectReferenceValue as Sprite);
            int selectedPortraitIndex = EditorGUI.Popup(position, property.displayName, previousPortraitIndex,
                portraitsArray.Select(p => p == null ? "<Null>" : p.name).ToArray());

            if (selectedPortraitIndex != previousPortraitIndex)
            {
                property.objectReferenceValue = portraitsArray[selectedPortraitIndex];
                property.serializedObject.ApplyModifiedProperties();
            }

            if (selectedPortraitIndex < portraitsArray.Length && selectedPortraitIndex >= 0)
            {
                return portraitsArray[selectedPortraitIndex];
            }
            else
            {
                return null;
            }
        }
    }
}
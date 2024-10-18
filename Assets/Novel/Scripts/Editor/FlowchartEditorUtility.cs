using Novel.Command;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public static Object FindObjectFromInstanceID(int instanceId)
        {
            try
            {
                var type = typeof(Object);
                var flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod;
                var ret = type.InvokeMember("FindObjectFromInstanceID", flags, null, null, new object[] {instanceId});
                return (Object) ret;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return null;
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
        /// <param name="folderPath">��������t�H���_�̃p�X</param>
        /// <param name="baseName">���O</param>
        /// <returns></returns>
        public static CommandData CreateCommandData(string folderPath, string baseName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var name = GetFileName(folderPath, baseName, "asset");
            var cmdData = ScriptableObject.CreateInstance<CommandData>();
            AssetDatabase.CreateAsset(cmdData, Path.Combine(folderPath, name));
            AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate);
            return cmdData;
        }

        /// <summary>
        /// �s�v��CommandData���폜���܂�
        /// </summary>
        public static void RemoveUnusedCommandData(string folderPath = null)
        {
            // Undo���Ŕj����Ԃ̃t�@�C�����폜 //
            var guids = AssetDatabase.FindAssets(null, new string[] { folderPath ??= ConstContainer.COMMANDDATA_PATH });
            int removeCount = 0;
            for(int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var obj = AssetDatabase.LoadAllAssetsAtPath(path);
                if(obj.Length == 0) // �A�Z�b�g���Ȃ� > �j�����Ă��� 
                {
                    File.Delete(path);
                    File.Delete($"{path}.meta");
                    removeCount++;
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // �ǂ̃t���[�`���[�g�ɂ������Ă��Ȃ�CommandData���폜 //
            var cmdDatas = GetAllScriptableObjects<CommandData>();
            var flowchartDatas = GetAllScriptableObjects<FlowchartData>();
            foreach (var cmdData in cmdDatas)
            {
                if (IsUsed(cmdData, flowchartDatas) == false)
                {
                    removeCount++;
                    DestroyScritableObject(cmdData);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            if(removeCount > 0)
            {
                Debug.Log($"�s�v�ȃf�[�^��{removeCount}�폜����܂���\nFolder: {folderPath}");
            }
            else
            {
                Debug.Log($"�s�v�ȃf�[�^�̌������������܂����B�폜���ꂽ�f�[�^�͂���܂���\nFolder: {folderPath}");
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

            if(selectedPortraitIndex < portraitsArray.Length && selectedPortraitIndex >= 0)
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
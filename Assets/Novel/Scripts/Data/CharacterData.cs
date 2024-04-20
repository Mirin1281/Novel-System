﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Novel
{
    [CreateAssetMenu(
        fileName = "Character",
        menuName = "ScriptableObject/Character")
    ]
    public class CharacterData : ScriptableObject
    {
        [field: SerializeField] public string CharacterName { get; private set; }
        [field: SerializeField] public Color NameColor { get; private set; } = Color.white;
        [field: SerializeField] public BoxType BoxType { get; private set; }
        [field: SerializeField] public PortraitType PortraitType { get; private set; }
        [SerializeField] Sprite[] portraits;
        public IEnumerable<Sprite> Portraits => portraits;

        /// <summary>
        /// エディタ用
        /// </summary>
        public static CharacterData GetCharacter(string characterName)
        {
            var characters = GetAllScriptableObjects<CharacterData>();
            var meetChara = characters.Where(c => c.CharacterName == characterName).ToList();
            if (meetChara.Count == 0)
            {
                Debug.LogWarning($"キャラクターが見つかりませんでした");
                return null;
            }
            else if(meetChara.Count == 1)
            {
                return meetChara[0];
            }
            else
            {
                Debug.LogWarning($"キャラクターのヒット数が多いです!: {meetChara.Count}");
                return null;
            }
        }

        static T[] GetAllScriptableObjects<T>(string folderName = null) where T : ScriptableObject
        {
#if UNITY_EDITOR
            string[] guids = folderName　== null
                ? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                : AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folderName });
            return guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
#else
            return null;
#endif
        }
    }
}
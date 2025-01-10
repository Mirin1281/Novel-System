using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Novel
{
    /// <summary>
    /// キャラクターの情報を格納するScriptableObject
    /// </summary>
    [CreateAssetMenu(
        fileName = "Character",
        menuName = ConstContainer.DATA_CREATE_PATH + "Character",
        order = 0)
    ]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] string characterName;
        [field: SerializeField] public Color NameColor { get; private set; } = Color.white;
        [field: SerializeField] public BoxType BoxType { get; private set; }
        [field: SerializeField] public PortraitType PortraitType { get; private set; }
        [SerializeField] Sprite[] portraits;

        /// <summary>
        /// ルビは消去されます(ルビが欲しい場合はNameIncludeRubyを使う)
        /// </summary>
        public string CharacterName => TagUtility.RemoveRubyText(characterName);
        public string NameIncludeRuby => characterName;
        public IEnumerable<Sprite> Portraits => portraits;

        /// <summary>
        /// (エディタ用)名前からキャラクターを取得します
        /// </summary>
        public static CharacterData GetCharacter(string characterName)
        {
#if UNITY_EDITOR
            var characters = GetAllScriptableObjects<CharacterData>();
            var meetCharas = characters.Where(c => c.CharacterName == characterName).ToArray();
            if (meetCharas.Length == 0)
            {
                Debug.LogWarning($"キャラクターが見つかりませんでした\n名前: {characterName}");
                return null;
            }
            else if (meetCharas.Length == 1)
            {
                return meetCharas[0];
            }
            else
            {
                Debug.LogWarning($"キャラクターが{meetCharas.Length}個ヒットしました");
                return meetCharas[0];
            }


            static T[] GetAllScriptableObjects<T>(string folderName = null) where T : ScriptableObject
            {
                string[] guids = folderName == null
                    ? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                    : AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folderName });
                return guids
                    .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToArray();
            }
#else
            return null;
#endif
        }
    }
}
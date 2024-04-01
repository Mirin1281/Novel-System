using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Novel
{
    [CreateAssetMenu(
        fileName = "CharacterData",
        menuName = "ScriptableObject/CharacterData")
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
            var characters = Resources.LoadAll<CharacterData>("Characters");
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
    }
}
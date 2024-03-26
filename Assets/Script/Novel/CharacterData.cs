using UnityEngine;
using System.Collections.Generic;

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
    }
}
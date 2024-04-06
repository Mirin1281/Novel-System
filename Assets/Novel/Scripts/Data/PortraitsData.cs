using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novel
{
    [CreateAssetMenu(
            fileName = "Portraits",
            menuName = "ScriptableObject/Portraits")
    ]
    public class PortraitsData : ScriptableObject
    {
        [field: SerializeField]
        public bool CreateOnSceneChanged { get; private set; }

        [field: Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます"), SerializeField]
        public List<LinkedPortrait> LinkedPortraitList { get; private set; }

        [Serializable]
        public class LinkedPortrait
        {
            [field: SerializeField, ReadOnly]
            public PortraitType Type { get; set; }

            [field: SerializeField]
            public Portrait Prefab { get; private set; }

            public Portrait Portrait { get; set; }
        }

        /// <summary>
        /// 列挙子を設定します
        /// </summary>
        [ContextMenu("◆SetEnum")]
        void SetEnum()
        {
            int enumCount = Enum.GetValues(typeof(PortraitType)).Length;
            if (LinkedPortraitList == null) LinkedPortraitList = new();
            int deltaCount = 1; // 仮置き
            while (deltaCount != 0)
            {
                deltaCount = LinkedPortraitList.Count - enumCount;
                if (deltaCount > 0)
                {
                    LinkedPortraitList.RemoveAt(enumCount);
                }
                else if (deltaCount < 0)
                {
                    LinkedPortraitList.Add(new LinkedPortrait());
                }
            }

            for (int i = 0; i < enumCount; i++)
            {
                LinkedPortraitList[i].Type = (PortraitType)i;
            }
        }
    }

    public enum PortraitType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("真白ノベル")] Type1,
        [InspectorName("河野修二")] Type2,
    }
}
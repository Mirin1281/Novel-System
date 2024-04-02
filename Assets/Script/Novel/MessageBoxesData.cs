using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novel
{
    // BoxTypeはこの下にあります

    [CreateAssetMenu(
        fileName = "MessageBoxesData",
        menuName = "ScriptableObject/MessageBoxesData")
    ]
    public class MessageBoxesData : ScriptableObject
    {
        [field: SerializeField, Tooltip(
                "true時はシーン切り替え時に全メッセージボックスを生成します\n" +
                "false時は受注生産方式でキャッシュします")]
        public bool CreateOnSceneChanged { get; private set; }

        [field: Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます"), SerializeField]
        public List<LinkedBox> LinkedBoxList { get; private set; }

        [Serializable]
        public class LinkedBox
        {
            [field: SerializeField, ReadOnly]
            public BoxType Type { get; set; }

            [field: SerializeField]
            public MessageBox Prefab { get; private set; }

            public MessageBox Box { get; set; }
        }

        /// <summary>
        /// 列挙子を設定します
        /// </summary>
        [ContextMenu("◆SetEnum")]
        void SetEnum()
        {
            int enumCount = Enum.GetValues(typeof(BoxType)).Length;
            if (LinkedBoxList == null) LinkedBoxList = new();
            int deltaCount = 1; // 仮置き
            while (deltaCount != 0)
            {
                deltaCount = LinkedBoxList.Count - enumCount;
                if (deltaCount > 0)
                {
                    LinkedBoxList.RemoveAt(enumCount);
                }
                else if (deltaCount < 0)
                {
                    LinkedBoxList.Add(new LinkedBox());
                }
            }

            for (int i = 0; i < enumCount; i++)
            {
                LinkedBoxList[i].Type = (BoxType)i;
            }
        }
    }

    public enum BoxType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("下")] Type1,
        [InspectorName("上")] Type2,
    }
}
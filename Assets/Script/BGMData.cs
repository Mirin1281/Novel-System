using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "BGMData",
    menuName = "ScriptableObject/BGMData")
]
public class BGMData : ScriptableObject
{
    [Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます")]
    [SerializeField] List<LinkedBGM> linkedBGMList;

    public LinkedBGM GetLinkedBGM(BGMType type) => linkedBGMList[(int)type];

#if UNITY_EDITOR
    /// <summary>
    /// 列挙子を設定します
    /// </summary>
    [ContextMenu("◆SetEnum")]
    void SetEnum()
    {
        int enumCount = Enum.GetValues(typeof(BGMType)).Length;
        if (linkedBGMList == null) linkedBGMList = new();
        int deltaCount = 1; // 仮置き
        while (deltaCount != 0)
        {
            deltaCount = linkedBGMList.Count - enumCount;
            if (deltaCount > 0)
            {
                linkedBGMList.RemoveAt(enumCount);
            }
            else if (deltaCount < 0)
            {
                linkedBGMList.Add(new LinkedBGM());
            }
        }

        for (int i = 0; i < enumCount; i++)
        {
            linkedBGMList[i].SetType((BGMType)i);
        }
    }
#endif
}

public enum BGMType
{
    [InspectorName("(デフォルト)なし")] None,
}

[Serializable]
public class LinkedBGM
{
    [field: SerializeField] public BGMType Type { get; private set; }
    [field: SerializeField] public AudioClip Clip { get; private set; }
    [field: SerializeField] public float Volume { get; private set; } = 1f;

#if UNITY_EDITOR
    public void SetType(BGMType type)
    {
        Type = type;
    }
#endif
}
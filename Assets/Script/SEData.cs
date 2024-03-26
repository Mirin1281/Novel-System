using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "SEData",
    menuName = "ScriptableObject/SEData")
]

public class SEData : ScriptableObject
{
    [Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます")]
    [SerializeField] List<LinkedSE> linkedSEList;

    public (AudioClip, float) GetSEAndVolume(SEType type)
        => linkedSEList[(int)type].GetSEAndVolume();

    [Serializable]
    class LinkedSE
    {
        [SerializeField] SEType type;
        [SerializeField] AudioClip clip;
        [SerializeField] float volume = 1f;

        public (AudioClip, float) GetSEAndVolume() => (clip, volume);

#if UNITY_EDITOR
        public void SetType(SEType type)
        {
            this.type = type;
        }
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// 列挙子を設定します
    /// </summary>
    [ContextMenu("◆SetEnum")]
    void SetEnum()
    {
        int enumCount = Enum.GetValues(typeof(SEType)).Length;
        if (linkedSEList == null) linkedSEList = new();
        int deltaCount = 1; // 仮置き
        while (deltaCount != 0)
        {
            deltaCount = linkedSEList.Count - enumCount;
            if (deltaCount > 0)
            {
                linkedSEList.RemoveAt(enumCount);
            }
            else if (deltaCount < 0)
            {
                linkedSEList.Add(new LinkedSE());
            }
        }

        for (int i = 0; i < enumCount; i++)
        {
            linkedSEList[i].SetType((SEType)i);
        }
    }
#endif
}

public enum SEType
{
    [InspectorName("なし")] None,
}
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Novel
{
    using TagType = TagUtility.TagType;
    using TagData = TagUtility.TagData;

    public class Writer : MonoBehaviour
    {
        [SerializeField] TMP_Text nameTmpro;
        [SerializeField] RubyTextMeshProUGUI storyTmpro;
        [SerializeField] MsgBoxInput input;
        float timePer100Charas; // 100文字表示するのにかかる時間(s)
        bool isSkipped;
        float defaultSpeed => GameManager.Instance.DefaultWriteSpeed;

        void Awake()
        {
            nameTmpro.SetText(string.Empty);
            storyTmpro.SetUneditedText(string.Empty);
            input.OnInputed += SkipTextIfValid;
        }

        void OnDestroy()
        {
            input.OnInputed -= SkipTextIfValid;
        }

        public async UniTask WriteAsync(
            CharacterData character, string fullText, CancellationToken token)
        {
            SetName(character);
            var (richText, tagDataList) = TagUtility.ExtractMyTag(fullText);
            await WriteStoryTextAsync(richText, tagDataList, null, token);
            SayLogger.AddLog(character, richText);


            async UniTask WriteStoryTextAsync(
                string richText, List<TagData> tagDataList = null, float? timePerCharas = null, CancellationToken token = default)
            {
                storyTmpro.SetUneditedText(richText);
                timePer100Charas = timePerCharas ?? defaultSpeed;
                token = token == default ? destroyCancellationToken : token;

                int tagNumber = 0;
                int insertIndex = -1;
                if (tagDataList != null && tagDataList.Count != 0)
                {
                    insertIndex = tagDataList[0].IndexIgnoreAllTag;
                }

                int i = 0;
                while(i <= richText.Length)
                {
                    storyTmpro.maxVisibleCharacters = i;

                    if (i == insertIndex)
                    {
                        int beforeIndex = insertIndex;
                        while (beforeIndex == insertIndex)
                        {
                            insertIndex = await ApplyTag(tagNumber, insertIndex);
                            tagNumber++;
                        }
                    }
                    await MyStatic.WaitSeconds(timePer100Charas / 100f, token);
                    i++;
                }


                async UniTask<int> ApplyTag(int num, int index)
                {
                    var value = tagDataList[num].Value;
                    var type = tagDataList[num].TagType;
                    if (type == TagType.None)
                    {

                    }
                    else if (type == TagType.SpeedStart)
                    {
                        if (isSkipped == false)
                        {
                            timePer100Charas /= value;
                        }
                    }
                    else if (type == TagType.SpeedEnd)
                    {
                        if (isSkipped == false)
                        {
                            timePer100Charas = defaultSpeed;
                        }
                    }
                    else if (type == TagType.WaitSeconds)
                    {
                        await WaitSecondsSkippable(value);
                    }
                    else if (type == TagType.WaitInput)
                    {
                        await WaitInput();
                    }
                    else if (type == TagType.WaitInputClear)
                    {
                        await WaitInput();
                        richText = richText.Remove(0, tagDataList[num].IndexIgnoreMyTag);
                        storyTmpro.SetUneditedText(richText);
                        i = 0;
                        storyTmpro.maxVisibleCharacters = 0;
                        foreach(var tagData in tagDataList)
                        {
                            tagData.IndexIgnoreAllTag -= index;
                        }
                    }
                    else if (type == TagType.RubyStart)
                    {
                        foreach (var tagData in tagDataList)
                        {
                            tagData.IndexIgnoreAllTag -= 4;
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }

                    // 次のタグがあればそこのインデックスを返す
                    if (num + 1 < tagDataList.Count)
                    {
                        return tagDataList[num + 1].IndexIgnoreAllTag;
                    }
                    return -1;
                }


                async UniTask WaitSecondsSkippable(float time)
                {
                    float t = 0f;
                    while (t < time && isSkipped == false)
                    {
                        t += Time.deltaTime;
                        await MyStatic.Yield(token);
                    }
                }

                async UniTask WaitInput()
                {
                    await input.WaitInput(() =>
                    {
                        timePer100Charas = defaultSpeed;
                        isSkipped = false;
                    },
                    token);
                }
            }
        }

        public void SetName(CharacterData character)
        {
            if (character != null)
            {
                nameTmpro.color = character.NameColor;
                nameTmpro.SetText(character.CharacterName);
            }
            else
            {
                nameTmpro.SetText(string.Empty);
            }
        }

        public void SkipTextIfValid()
        {
            timePer100Charas = 0;
            isSkipped = true;
        }

        /// <summary>
        /// エディタ用
        /// </summary>
        public void PreviewText(CharacterData character, string text)
        {
            var convertedText = TagUtility.ExtractMyTag(text).convertedText;
            storyTmpro.SetUneditedText(convertedText);
            SetName(character);
        }
    }
}
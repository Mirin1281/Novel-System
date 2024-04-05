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
        float defaultSpeed => NovelManager.Instance.DefaultWriteSpeed;

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
            isSkipped = false;
            //tagDataList.ForEach(data => data.ShowTagStatus()); // デバッグ用
            await WriteStoryTextAsync(richText, tagDataList, null, token);
            SayLogger.AddLog(character, richText);


            async UniTask WriteStoryTextAsync(
                string richText, List<TagData> tagDataList = null, float? timePerCharas = null, CancellationToken token = default)
            {
                storyTmpro.SetUneditedText(richText);
                storyTmpro.ForceMeshUpdate();
                var planeText = storyTmpro.GetParsedText();

                timePer100Charas = timePerCharas ?? defaultSpeed;
                token = token == default ? this.GetCancellationTokenOnDestroy() : token;

                int insertIndex = -1;
                if (tagDataList != null && tagDataList.Count != 0)
                {
                    insertIndex = tagDataList[0].IndexIgnoreAllTag;
                }
                int tagNumber = 0;

                int i = 0;
                while(i <= planeText.Length)
                {
                    storyTmpro.maxVisibleCharacters = i;

                    if (i == insertIndex)
                    {
                        int beforeIndex = insertIndex;
                        while (beforeIndex == insertIndex)
                        {
                            await ApplyTag(tagDataList[tagNumber]);
                            tagNumber++;
                            if(tagNumber < tagDataList.Count)
                            {
                                insertIndex = tagDataList[tagNumber].IndexIgnoreAllTag;
                            }
                            else
                            {
                                insertIndex = -1;
                            }
                        }
                    }
                    await Wait.Seconds(timePer100Charas / 100f, token);
                    i++;
                }


                async UniTask ApplyTag(TagData tag)
                {
                    var type = tag.TagType;
                    if (type == TagType.None)
                    {

                    }
                    else if (type == TagType.SpeedStart)
                    {
                        if (isSkipped == false)
                        {
                            timePer100Charas /= tag.Value;
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
                        await WaitSecondsSkippable(tag.Value);
                    }
                    else if (type == TagType.WaitInput)
                    {
                        await WaitInput();
                    }
                    else if (type == TagType.WaitInputClear)
                    {
                        await WaitInput();
                        richText = richText.Remove(0, tag.IndexIgnoreMyTag);
                        storyTmpro.SetUneditedText(richText);
                        storyTmpro.ForceMeshUpdate();
                        planeText = storyTmpro.GetParsedText();
                        i = 0;
                        storyTmpro.maxVisibleCharacters = 0;
                        int delta = tag.IndexIgnoreAllTag;
                        foreach (var tagData in tagDataList)
                        {
                            tagData.IndexIgnoreAllTag -= delta;
                            tagData.IndexIgnoreMyTag -= delta;
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
                }


                async UniTask WaitSecondsSkippable(float time)
                {
                    float t = 0f;
                    while (t < time && isSkipped == false)
                    {
                        t += Time.deltaTime;
                        await Wait.Yield(token);
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

        void SetName(CharacterData character)
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

        void SkipTextIfValid()
        {
            timePer100Charas = 0;
            isSkipped = true;
        }

#if UNITY_EDITOR
        public void PreviewText(CharacterData character, string text)
        {
            var convertedText = TagUtility.ExtractMyTag(text).convertedText;
            storyTmpro.SetUneditedText(convertedText);
            SetName(character);
        }
#endif
    }
}
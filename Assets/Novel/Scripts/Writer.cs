using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using System;
using System.Threading;
using System.Collections.Generic;
using TagType = Novel.TagUtility.TagType;
using TagData = Novel.TagUtility.TagData;
using System.Text.RegularExpressions;

namespace Novel
{
    public class Writer : MonoBehaviour
    {
        [SerializeField] RubyTextMeshProUGUI nameTmpro;
        [SerializeField] RubyTextMeshProUGUI storyTmpro;
        [SerializeField] MessageBoxInput input;
        float timePer100Charas; // 100文字表示するのにかかる時間(s)
        bool isSkipped;
        float DefaultSpeed => NovelManager.Instance.DefaultWriteSpeed;

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
            CharacterData character, string fullText, CancellationToken token, bool wholeShow = false)
        {
            await WriteAsync(character, character.CharacterName, fullText, token, wholeShow);
        }
        public async UniTask WriteAsync(
            CharacterData character, string nameText, string fullText, CancellationToken token, bool wholeShow = false)
        {
            SetName(character, nameText);
            if(NovelManager.Instance.IsUseRuby == false)
            {
                fullText = RemoveRubyText(fullText);
            }
            var (richText, tagDataList) = TagUtility.ExtractMyTag(fullText);
            isSkipped = false;
            //tagDataList.ForEach(data => data.ShowTagStatus()); // デバッグ用
            timePer100Charas = wholeShow ? 0 : DefaultSpeed;
            await WriteStoryTextAsync(richText, tagDataList, token);
            SayLogger.AddLog(character.CharacterName, richText);


            async UniTask WriteStoryTextAsync(string richText, List<TagData> tagDataList, CancellationToken token)
            {
                storyTmpro.SetUneditedText(richText);
                storyTmpro.ForceMeshUpdate();
                var planeText = storyTmpro.GetParsedText();

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
                            timePer100Charas = DefaultSpeed;
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
                        await UniTask.Yield(token);
                    }
                }

                async UniTask WaitInput()
                {
                    await input.WaitInput(() =>
                    {
                        timePer100Charas = DefaultSpeed;
                        isSkipped = false;
                    },
                    token);
                }
            }
        }

        string RemoveRubyText(string text)
        {
            string regexString = @"\<r=.*?\>";
            text = Regex.Replace(text, regexString, string.Empty);
            text = text.Replace("</r>", string.Empty);
            return text;
        }

        void SetName(CharacterData character, string nameText)
        {
            string name = null;
            if (character != null)
            {
                nameTmpro.color = character.NameColor;
                if (NovelManager.Instance != null && NovelManager.Instance.IsUseRuby == false)
                {
                    name = RemoveRubyText(nameText);
                }
                else
                {
                    name = nameText;
                }
            }
            nameTmpro.SetUneditedText(name);
        }

        void SkipTextIfValid()
        {
            timePer100Charas = 0;
            isSkipped = true;
        }

#if UNITY_EDITOR
        public void PreviewText(CharacterData character, string text)
        {
            var richText = TagUtility.ExtractMyTag(text).convertedText;
            storyTmpro.SetUneditedText(richText);
            SetName(character, character.NameIncludeRuby);
        }
#endif
    }
}
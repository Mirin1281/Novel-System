using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using System.Threading;
using System.Collections.Generic;
using TagType = Novel.TagUtility.TagType;
using TagData = Novel.TagUtility.TagData;

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
            if (input == null)
            {
                GetComponent<MessageBoxInput>();
            }
            nameTmpro.SetText(string.Empty);
            storyTmpro.SetUneditedText(string.Empty);
            input.OnInputed += SkipTextIfValid;


            void SkipTextIfValid()
            {
                timePer100Charas = 0;
                isSkipped = true;
            }
        }

        public UniTask WriteAsync(CharacterData character, string fullText, CancellationToken token, bool wholeShow = false)
        {
            return WriteAsync(character, character.CharacterName, fullText, token, wholeShow);
        }

        public async UniTask WriteAsync(CharacterData character, string nameText, string fullText, CancellationToken token, bool wholeShow = false)
        {
            ClearText();
            // 名前と色の設定 //
            nameText = string.IsNullOrEmpty(nameText) ?
                (character == null ?
                    null :
                    character.NameIncludeRuby) :
                nameText;
            if (NovelManager.Instance != null && NovelManager.Instance.IsUseRuby == false)
            {
                nameText = TagUtility.RemoveRubyText(nameText);
            }
            var nameColor = character == null ? Color.white : character.NameColor;
            SetName(nameText, nameColor);

            // テキストの設定 //

            if (NovelManager.Instance.IsUseRuby == false)
            {
                fullText = TagUtility.RemoveRubyText(fullText);
            }
            var (richText, tagDataList) = TagUtility.ExtractMyTag(fullText);
            isSkipped = false;
            timePer100Charas = wholeShow ? 0 : DefaultSpeed;
            await WriteTextAsync(richText, tagDataList, token);
            //SayLogger.AddLog(nameText, richText); // ログを記録する場合


            async UniTask WriteTextAsync(string richText, List<TagData> tagDataList, CancellationToken token)
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
                int rubyDelta = 0;

                for (int i = 0; i < planeText.Length; i++)
                {
                    storyTmpro.maxVisibleCharacters = i + 1;

                    if (i == insertIndex)
                    {
                        int beforeIndex = insertIndex;
                        while (beforeIndex == insertIndex)
                        {
                            await ApplyTag(tagDataList[tagNumber]);
                            tagNumber++;
                            if (tagNumber < tagDataList.Count)
                            {
                                insertIndex = tagDataList[tagNumber].IndexIgnoreAllTag + rubyDelta;
                            }
                            else
                            {
                                insertIndex = -1;
                            }
                        }
                    }
                    await AsyncUtility.Seconds(timePer100Charas / 100f, token);
                }


                async UniTask ApplyTag(TagData tag)
                {
                    var type = tag.TagType;
                    if (type == TagType.SpeedStart)
                    {
                        if (isSkipped == false)
                        {
                            timePer100Charas = wholeShow ? 0 : timePer100Charas / tag.Value;
                        }
                    }
                    else if (type == TagType.SpeedEnd)
                    {
                        if (isSkipped == false)
                        {
                            timePer100Charas = wholeShow ? 0 : DefaultSpeed;
                        }
                    }
                    else if (type == TagType.WaitSeconds)
                    {
                        await WaitSecondsSkippable(wholeShow ? 0 : tag.Value);
                    }
                    else if (type == TagType.WaitInput)
                    {
                        await WaitInput();
                    }
                    else if (type == TagType.RubyStart)
                    {
                        // "<r=>"の文字数分引く
                        rubyDelta -= 4;
                    }
                    else if (type == TagType.RubyEnd)
                    {
                        // "</r>"の文字数分引く
                        rubyDelta -= 4;
                    }
                    else
                    {
                        Debug.LogWarning("タグが存在しませんでした\n" + type);
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
                        timePer100Charas = wholeShow ? 0 : DefaultSpeed;
                        isSkipped = false;
                    },
                    token);
                }
            }
        }

        void SetName(string nameText, Color nameColor)
        {
            nameTmpro.color = nameColor;
            nameTmpro.SetUneditedText(nameText);
        }

        public void ClearText()
        {
            nameTmpro.SetUneditedText(string.Empty);
            storyTmpro.SetUneditedText(string.Empty);
        }

#if UNITY_EDITOR
        public void PreviewText(Color nameColor, string nameText, string text)
        {
            var richText = TagUtility.ExtractMyTag(text).convertedText;
            storyTmpro.SetUneditedText(richText);
            SetName(nameText, nameColor);
        }
#endif
    }
}
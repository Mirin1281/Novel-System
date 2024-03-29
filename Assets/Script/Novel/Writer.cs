using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using System;
using System.Threading;

namespace Novel
{
using TagType = TagUtility.TagType;

    public class Writer : MonoBehaviour
    {
        [SerializeField] TMP_Text nameTmpro;
        [SerializeField] TMP_Text storyTmpro;
        [SerializeField] MsgBoxInput input;
        float timePer100Charas; // 100文字表示するのにかかる時間(s)
        bool isSkipped;
        float defaultSpeed => GameManager.Instance.DefaultWriteSpeed;

        void Awake()
        {
            nameTmpro.SetText(string.Empty);
            storyTmpro.SetText(string.Empty);
            input.OnInputed += SkipTextIfValid;
        }

        public async UniTask WriteAsync(
            CharacterData character, string fullText, CancellationToken token)
        {
            SetName(character);
            var (convertedText, tagDataArray) = TagUtility.ExtractTag(fullText);
            await WriteStoryTextAsync(convertedText, tagDataArray, null, token);
            SayLogger.AddLog(character, convertedText);


            async UniTask WriteStoryTextAsync(
                string text, TagUtility.TagData[] tagDataArray = null, float? timePerCharas = null, CancellationToken token = default)
            {
                storyTmpro.SetText(text);
                timePer100Charas = timePerCharas ?? defaultSpeed;
                int tagNumber = 0;
                int tagInsertIndex = -1;
                if (tagDataArray != null)
                {
                    tagInsertIndex = tagDataArray[tagNumber].Index;
                }
                int i = 0;
                while(i <= text.Length)
                {
                    storyTmpro.maxVisibleCharacters = i;
                    if (i == tagInsertIndex)
                    {
                        tagInsertIndex = await ApplyTag(tagNumber, tagInsertIndex);
                        tagNumber++;
                    }
                    await MyStatic.WaitSeconds(timePer100Charas / 100f,
                        token == default ? destroyCancellationToken : token);
                    i++;
                }

                async UniTask<int> ApplyTag(int tagNum, int tagIndex)
                {
                    var value = tagDataArray[tagNum].Value;
                    var type = tagDataArray[tagNum].TagType;
                    if(type == TagType.None)
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
                        text = text.Remove(0, tagIndex);
                        storyTmpro.SetText(text);
                        i = 0;
                        storyTmpro.maxVisibleCharacters = i;
                        for(int j = 0; j < tagDataArray.Length; j++)
                        {
                            tagDataArray[j].Index -= tagIndex;
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }

                    // 次のタグがあればそこのインデックスを返す
                    if (tagNum + 1 < tagDataArray.Length)
                    {
                        tagIndex = tagDataArray[tagNum + 1].Index;
                    }
                    return tagIndex;
                }


                async UniTask WaitSecondsSkippable(float time)
                {
                    float t = 0f;
                    while (t < time && isSkipped == false)
                    {
                        t += Time.deltaTime;
                        await MyStatic.Yield(destroyCancellationToken);
                    }
                }

                async UniTask WaitInput()
                {
                    await input.WaitInput(() =>
                    {
                        timePer100Charas = defaultSpeed;
                        isSkipped = false;
                    },
                    destroyCancellationToken);
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

        public void PreviewText(CharacterData character, string text)
        {
            var convertedText = TagUtility.ExtractTag(text).convertedText;
            storyTmpro.SetText(convertedText);
            SetName(character);
        }

        void OnDestroy()
        {
            input.OnInputed -= SkipTextIfValid;
        }
    }
}
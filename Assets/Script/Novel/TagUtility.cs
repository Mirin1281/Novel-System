using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace Novel
{
    public static class TagUtility
    {
        const string RegexString = @"\{.*?\}";

        // 【タグの増やし方】
        // 1. TagTypeの項目を増やす
        // 2. 下部のGetTagStatus()ないに条件を増やす
        // 3. Writerクラス内の処理を増やす
        public enum TagType
        {
            None,
            SpeedStart,
            SpeedEnd,
            WaitSeconds,
            WaitInput,
            WaitInputClear,
        }

        public struct TagData
        {
            public TagType TagType; // タグの種類
            public float Value; // タグの値(ない場合もある)
            public int Index; // タグを無視した時のタグの位置(語彙力)

            public TagData(TagType tagType, float value, int index)
            {
                TagType = tagType;
                Value = value;
                Index = index;
            }
        }

        /// <summary>
        /// テキストからタグを抽出します
        /// </summary>
        /// <param name="text"></param>
        /// <returns>(タグを取り除いたテキスト, TagDataの配列)</returns>
        public static (string convertedText, TagData[] tagDataArray) ExtractTag(string text)
        {
            var regex = new Regex(RegexString);
            var matches = regex.Matches(text);
            if (matches.Count == 0) return (text, null);

            var tagDataArray = new TagData[matches.Count];
            int tagStringLength = 0;
            int count = 0;
            foreach (Match match in matches)
            {
                string tag = match.Value;
                var s = tag.Remove(0, 1).Remove(tag.Length - 2);

                var (tagType, value) = GetTagStatus(s);

                tagDataArray[count] = new TagData(tagType, value, match.Index - tagStringLength);
                tagStringLength += tag.Length;
                count++;
            }
            return (regex.Replace(text, string.Empty), tagDataArray);


            static (TagType, float) GetTagStatus(string tagContent)
            {
                TagType tagType = TagType.None;
                float value = 0f;
                if (tagContent.StartsWith("s=", StringComparison.Ordinal))
                {
                    tagType = TagType.SpeedStart;
                    value = float.Parse(tagContent.Replace("s=", string.Empty));
                }
                else if (tagContent == "/s")
                {
                    tagType = TagType.SpeedEnd;
                }
                else if (tagContent.StartsWith("w=", StringComparison.Ordinal))
                {
                    tagType = TagType.WaitSeconds;
                    value = float.Parse(tagContent.Replace("w=", string.Empty));
                }
                else if (tagContent == "wi")
                {
                    tagType = TagType.WaitInput;
                }
                else if (tagContent == "wic")
                {
                    tagType = TagType.WaitInputClear;
                }
                else
                {
                    Debug.LogWarning($"{{{tagContent}}} ←タグのタイポしてるかも");
                }
                return (tagType, value);
            }
        }
    }
}
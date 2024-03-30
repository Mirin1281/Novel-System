using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Novel
{
    public static class TagUtility
    {
        const string RegexString = @"\<.*?\>";

        // 【タグの増やし方】
        // 1. TagTypeの項目を増やす
        // 2. 下部のGetTagStatus()の中に条件を増やす
        // 3. Writerクラス内の処理を増やす
        public enum TagType
        {
            None,
            SpeedStart,
            SpeedEnd,
            WaitSeconds,
            WaitInput,
            WaitInputClear,
            RubyStart,
        }

        public class TagData
        {
            public TagType TagType; // タグの種類
            public float Value; // タグの値(ない場合もある)
            public readonly int IndexIgnoreMyTag; // 独自のタグを無視した時のタグの位置(語彙力)
            public int IndexIgnoreAllTag; // リッチテキストを含む全てのタグを無視した時のタグの位置

            public TagData(TagType tagType, float value, int indexIgnoreMyTag, int indexIgnoreAllTag)
            {
                TagType = tagType;
                Value = value;
                IndexIgnoreMyTag = indexIgnoreMyTag;
                IndexIgnoreAllTag = indexIgnoreAllTag;
            }

            public void ShowTagStatus()
            {
                Debug.Log($"type: {TagType}, allIndex: {IndexIgnoreAllTag}");
            }
        }

        /// <summary>
        /// テキストからタグを抽出します
        /// </summary>
        /// <param name="text"></param>
        /// <returns>(タグを取り除いたテキスト, TagDataの配列)</returns>
        public static (string convertedText, List<TagData> tagDataList) ExtractMyTag(string text)
        {
            var regex = new Regex(RegexString);
            var matches = regex.Matches(text);
            if (matches.Count == 0) return (text, null);

            var tagDataList = new List<TagData>();
            int myTagsLength = 0;
            int allTagsLength = 0;
            foreach (Match match in matches)
            {
                string tag = match.Value;
                
                var (tagType, value) = GetTagStatus(tag);

                if (tagType != TagType.None)
                {
                    tagDataList.Add(new TagData(tagType, value, match.Index - myTagsLength, match.Index - allTagsLength));
                    if (tagType == TagType.RubyStart) continue;
                    text = text.Replace(match.Value, string.Empty);
                    myTagsLength += tag.Length;
                }
                allTagsLength += tag.Length;
            }
            return (text, tagDataList);


            static (TagType, float) GetTagStatus(string tag)
            {
                // "<>"を取り除く
                var content = tag.Remove(0, 1).Remove(tag.Length - 2);

                TagType tagType = TagType.None;
                float value = 0f;
                if (content.StartsWith("sp=", StringComparison.Ordinal))
                {
                    tagType = TagType.SpeedStart;
                    value = float.Parse(content.Replace("sp=", string.Empty));
                }
                else if (content == "/sp")
                {
                    tagType = TagType.SpeedEnd;
                }
                else if (content.StartsWith("w=", StringComparison.Ordinal))
                {
                    tagType = TagType.WaitSeconds;
                    value = float.Parse(content.Replace("w=", string.Empty));
                }
                else if (content == "wi")
                {
                    tagType = TagType.WaitInput;
                }
                else if (content == "wic")
                {
                    tagType = TagType.WaitInputClear;
                }
                else if (content.StartsWith("r=", StringComparison.Ordinal))
                {
                    tagType = TagType.RubyStart;
                    value = content.Replace("r=", string.Empty).Length;
                }
                return (tagType, value);
            }
        }
    }
}
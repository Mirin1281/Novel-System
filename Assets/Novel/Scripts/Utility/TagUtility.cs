using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Novel
{
    public static class TagUtility
    {
        const string RegexString = @"\<.*?\>";

        // 【タグの増やし方】
        // 1. 直下のTagTypeの項目を増やす
        // 2. 下部のGetTagStatus()の中に条件を増やす
        // 3. Writerクラス内(のApplyTagメソッドの中)の処理を増やす
        public enum TagType
        {
            None,
            SpeedStart,
            SpeedEnd,
            WaitSeconds,
            WaitInput,
            RubyStart,
            RubyEnd,
        }

        public readonly struct TagData : IEquatable<TagData>
        {
            public readonly TagType TagType;
            public readonly float Value;

            /// <summary>
            /// リッチテキストを含む全てのタグを無視した時のタグの位置
            /// </summary>
            public readonly int IndexIgnoreAllTag;

            public TagData(TagType tagType, float value, int indexIgnoreAllTag)
            {
                TagType = tagType;
                Value = value;
                IndexIgnoreAllTag = indexIgnoreAllTag;
            }

            public bool Equals(TagData other)
            {
                return TagType == other.TagType
                    && Value == other.Value
                    && IndexIgnoreAllTag == other.IndexIgnoreAllTag;
            }

            public override bool Equals(object obj)
            {
                if (obj is TagData)
                    return Equals((TagData)obj);
                return false;
            }

            public override readonly int GetHashCode()
            {
                return HashCode.Combine(TagType, Value, IndexIgnoreAllTag);
            }
        }

        public static string RemoveRubyText(string text)
        {
            string regexString = @"\<r=.*?\>";
            text = Regex.Replace(text, regexString, string.Empty);
            text = text.Replace("</r>", string.Empty);
            return text;
        }

        public static string RemoveSizeTag(string text)
        {
            text ??= string.Empty;
            var matches = Regex.Matches(text, RegexString);
            if (matches.Count == 0) return text;
            var match = matches[0];
            while (match.Success)
            {
                if (match.Value == "</size>" || match.Value.StartsWith("<size=", StringComparison.Ordinal))
                {
                    text = text.Replace(match.Value, string.Empty);
                }
                match = match.NextMatch();
            }
            return text;
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
                    tagDataList.Add(new TagData(tagType, value, match.Index - allTagsLength));
                    if (tagType is TagType.RubyStart or TagType.RubyEnd) continue;
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
                if (content.StartsWith("s=", StringComparison.Ordinal))
                {
                    tagType = TagType.SpeedStart;
                    value = float.Parse(content.Replace("s=", string.Empty));
                }
                else if (content == "/s")
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
                else if (content.StartsWith("r=", StringComparison.Ordinal))
                {
                    tagType = TagType.RubyStart;
                }
                else if (content == "/r")
                {
                    tagType = TagType.RubyEnd;
                }
                return (tagType, value);
            }
        }
    }
}
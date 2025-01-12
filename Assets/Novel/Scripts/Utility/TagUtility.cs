using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

namespace Novel
{
    public static class TagUtility
    {
        public const string TagRegexString = @"\<.*?\>";

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

        public readonly struct TagData
        {
            public readonly TagType TagType;
            public readonly float Value;

            /// <summary>
            /// リッチテキストを含む他の全てのタグを無視した時のタグの位置
            /// </summary>
            public readonly int IndexIgnoreAllTag;

            public TagData(TagType tagType, float value, int indexIgnoreAllTag)
            {
                TagType = tagType;
                Value = value;
                IndexIgnoreAllTag = indexIgnoreAllTag;
            }
        }

        public static string RemoveRubyText(string text)
        {
            StringBuilder result = new StringBuilder();
            int length = text.Length;
            int i = 0;

            while (i < length)
            {
                if (i + 2 < length && text[i] == '<' && text[i + 1] == 'r' && text[i + 2] == '=')
                {
                    // "<r="が見つかった場合、対応する閉じタグ ">" を探す
                    int endIndex = text.IndexOf('>', i);
                    if (endIndex != -1)
                    {
                        // ">" の次の文字に移動
                        i = endIndex + 1;
                        continue;
                    }
                }
                else if (i + 4 < length && text.Substring(i, 4) == "</r>")
                {
                    // "</r>" が見つかった場合、スキップ
                    i += 4;
                    continue;
                }
                else
                {
                    // それ以外の文字は結果に追加
                    result.Append(text[i]);
                    i++;
                }
            }

            return result.ToString();
        }

#if UNITY_EDITOR
        // エディタのサイズタグが暴発するため
        public static string RemoveSizeTag(string text)
        {
            var regex = new Regex(TagRegexString);
            var matches = regex.Matches(text);
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
#endif

        /// <summary>
        /// テキストからタグを抽出します
        /// </summary>
        /// <param name="text"></param>
        /// <returns>(タグを取り除いたテキスト, TagDataの配列)</returns>
        public static (string convertedText, List<TagData> tagDataList) ExtractMyTag(string text)
        {
            var regex = MessageBoxManager.Instance == null ? new Regex(TagRegexString) : MessageBoxManager.Instance.TagRegex;
            var matches = regex.Matches(text);
            if (matches.Count == 0) return (text, null);

            var tagDataList = new List<TagData>();
            int myTagsLength = 0;
            int allTagsLength = 0;
            foreach (Match match in matches)
            {
                string tagStr = match.Value;
                var (tagType, value) = GetTagStatus(tagStr);

                if (tagType != TagType.None)
                {
                    tagDataList.Add(new TagData(tagType, value, match.Index - allTagsLength));
                    if (tagType is TagType.RubyStart or TagType.RubyEnd) continue;
                    text = text.Replace(match.Value, string.Empty);
                    myTagsLength += tagStr.Length;
                }
                allTagsLength += tagStr.Length;
            }
            return (text, tagDataList);


            static (TagType, float) GetTagStatus(string tag)
            {
                // "<>"を取り除く
                string content = tag[1..^1];

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
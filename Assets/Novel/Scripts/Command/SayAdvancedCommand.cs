using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Say(Advanced)"), System.Serializable]
    public class SayAdvancedCommand : SayCommand
    {
        [SerializeField]
        float boxShowTime;

        [SerializeField, Tooltip(
            "\"<flag0>万円\"のように書くことで、文の中にフラグの値を入れ込むことができます\n" +
            "(FlagKeyの型はint型以外でもかまいません)")]
        FlagKeyDataBase[] flagKeys;

        [SerializeField]
        string characterName;

        protected override async UniTask SayAsync(string text, string characterName = null, float boxShowTime = 0f)
        {
            var convertedText = ReplaceFlagValue(text, flagKeys);
            await base.SayAsync(convertedText, this.characterName, this.boxShowTime);
        }

        /// <summary>
        /// "<flag0>"などの部分をそこに対応する変数値に置き換えます
        /// </summary>
        string ReplaceFlagValue(string fullText, FlagKeyDataBase[] flagKeys)
        {
            for (int i = 0; i < flagKeys.Length; i++)
            {
                if (fullText.Contains($"<flag{i}>"))
                {
                    fullText = fullText.Replace($"<flag{i}>",
                        FlagManager.GetFlagValueString(flagKeys[i]).valueStr);
                }
                else
                {
                    Debug.LogWarning($"<flag{i}>がなかったよ");
                }
            }
            return fullText;
        }
    }
}
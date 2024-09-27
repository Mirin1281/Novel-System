using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Say(Advanced)"), System.Serializable]
    public class SayAdvanced : Say
    {
        [SerializeField]
        BoxType boxType;

        [SerializeField]
        float boxShowTime;

        [SerializeField]
        string characterName;

        [SerializeField, Tooltip(
            "\"<flag0>万円\"のように書くことで、文の中にフラグの値を入れ込むことができます\n" +
            "(FlagKeyの型はint型以外でもかまいません)")]
        FlagKeyDataBase[] flagKeys;

        protected override async UniTask SayAsync(string text, string characterName = null, float boxShowTime = 0f, BoxType boxType = BoxType.Default)
        {
            var convertedText = ReplaceFlagValue(text, flagKeys);
            await base.SayAsync(convertedText, this.characterName, this.boxShowTime, this.boxType);
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
                    if(FlagManager.TryGetFlagValueString(flagKeys[i], out string valueStr))
                    {
                        fullText = fullText.Replace($"<flag{i}>", valueStr);
                    }
                    else
                    {
                        fullText = fullText.Replace($"<flag{i}>", string.Empty);
                    }
                }
                else
                {
                    Debug.LogWarning($"<flag{i}>がありませんでした");
                }
            }
            return fullText;
        }

        protected override string GetCharacterName()
        {
            var baseName = base.GetCharacterName();
            return string.IsNullOrEmpty(characterName) ? baseName : characterName;
        }
    }
}
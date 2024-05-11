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
            "\"<flag0>���~\"�̂悤�ɏ������ƂŁA���̒��Ƀt���O�̒l����ꍞ�ނ��Ƃ��ł��܂�\n" +
            "(FlagKey�̌^��int�^�ȊO�ł����܂��܂���)")]
        FlagKeyDataBase[] flagKeys;

        [SerializeField]
        string characterName;

        protected override async UniTask SayAsync(string text, string characterName = null, float boxShowTime = 0f)
        {
            var convertedText = ReplaceFlagValue(text, flagKeys);
            await base.SayAsync(convertedText, this.characterName, this.boxShowTime);
        }

        /// <summary>
        /// "<flag0>"�Ȃǂ̕����������ɑΉ�����ϐ��l�ɒu�������܂�
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
                    Debug.LogWarning($"<flag{i}>���Ȃ�������");
                }
            }
            return fullText;
        }
    }
}
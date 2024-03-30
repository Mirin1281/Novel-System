using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Say"), System.Serializable]
    public class SayCommand : CommandBase
    {
        [SerializeField]
        CharacterData character;

        [SerializeField, Tooltip("立ち絵を変更できます")]
        Sprite changeSprite;

        [SerializeField, TextArea]
        string storyText;

        [SerializeField]
        float boxShowTime;

        [SerializeField, Tooltip(
            "\"<item0>万円\"のように書くことで、文の中にフラグの値を入れ込むことができます\n" +
            "(FlagKeyの型はint型以外でもかまいません)")]
        FlagKeyDataBase[] flagKeys;

        // キャラクターが設定されない時に使われるメッセージボックスのタイプ
        const BoxType DefaultType = BoxType.Default;

        protected override async UniTask EnterAsync()
        {
            var convertedText = ReplaceFlagValue(storyText, flagKeys);

            // 立ち絵の変更 
            // 表示とかはPortraitでやって、こっちはチェンジだけって感じ
            if (changeSprite != null && character != null)
            {
                var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
                if (portrait.gameObject.activeInHierarchy == false)
                {
                    Debug.LogWarning("立ち絵が表示されていません！");
                }
                portrait.SetSprite(changeSprite);
            }

            BoxType boxType = character == null ? DefaultType : character.BoxType;
            MessageBoxManager.Instance.OtherClearFadeAsync(boxType, 0f).Forget();
            var msgBox = MessageBoxManager.Instance.CreateIfNotingBox(boxType);
            if(msgBox.gameObject.activeInHierarchy == false)
            {
                await msgBox.ShowFadeAsync(boxShowTime, CallStatus.Token);
            }
            await msgBox.Writer.WriteAsync(character, convertedText, CallStatus.Token);
            await msgBox.Input.WaitInput(token: CallStatus.Token);
        }

        /// <summary>
        /// "<item0>"などの部分をそこに対応する変数値に置き換えます
        /// </summary>
        string ReplaceFlagValue(string fullText, FlagKeyDataBase[] flagKeys)
        {
            for (int i = 0; i < flagKeys.Length; i++)
            {
                if (fullText.Contains($"<item{i}>"))
                {
                    fullText = fullText.Replace($"<item{i}>",
                        FlagManager.GetFlagValueString(flagKeys[i]).valueStr);
                }
                else
                {
                    Debug.LogWarning($"<item{i}>がなかったよ");
                }
            }
            return fullText;
        }

        #region For EditorWindow

        protected override string GetSummary()
        {
            if(string.IsNullOrEmpty(storyText))
            {
                return WarningColorText();
            }
            int index = storyText.IndexOf("\n");
            var charaName = character == null ? string.Empty : $"{character.CharacterName} ";
            if (index == -1)
            {
                return $"{charaName}\"{storyText}\"";
            }
            else
            {
                return $"{charaName}\"{storyText.Remove(index)}\"";
            }
        }

        protected override Color GetCommandColor() => new Color32(235, 210, 225, 255);

        #endregion
    }
}
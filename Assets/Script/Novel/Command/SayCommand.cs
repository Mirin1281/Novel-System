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

        [SerializeField, Tooltip("\"{item0}万円\"のように書くことで、文の中にフラグの値を入れ込むことができます")]
        FlagKeyDataBase[] flagKeys;

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

            BoxType boxType = character == null ? BoxType.Default : character.BoxType;
            MessageBoxManager.Instance.OtherClearFadeAsync(boxType, 0f).Forget();
            var msgBox = MessageBoxManager.Instance.CreateIfNotingBox(boxType);
            if(msgBox.gameObject.activeInHierarchy == false)
            {
                await msgBox.ShowFadeAsync();
            }
            await msgBox.Writer.WriteAsync(character, convertedText);
            await msgBox.Input.WaitInput();
        }

        /// <summary>
        /// "{item0}"などの部分をそこに対応する変数値に置き換えます
        /// </summary>
        /// <param name="text"></param>
        /// <param name="flagKeys"></param>
        /// <returns></returns>
        string ReplaceFlagValue(string text, FlagKeyDataBase[] flagKeys)
        {
            for (int i = 0; i < flagKeys.Length; i++)
            {
                if (text.Contains($"{{item{i}}}"))
                {
                    text = text.Replace($"{{item{i}}}",
                        FlagManager.GetFlagValueString(flagKeys[i]).valueStr);
                }
                else
                {
                    Debug.LogWarning($"{{item{i}}}がなかったよ");
                }
            }
            return text;
        }

        /* 【文字装飾についての説明】
         * 文の中にはTMPro由来のリッチテキストを入れることができ、文字の色やサイズを変えられます
         * また、その他に{hoge}という構文で独自の設定(タグ)を使用できます
         * 
         * 【タグの例】
         * {s=1.2} 表示スピードを変更します(数値は何倍にするかを表します)
         * {/s} 表示スピードをデフォルトに戻します
         * {w=1} その場所で1秒待機します(入力があったら無視されます)
         * {wi} 入力があるまで待機します(入力があった時もそこで止まります)
         * {wic} 入力があるまで待機し、今までの文をクリアしてから次を表示します(入力があった時もそこで止まります)
         */

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

        protected override Color GetCommandColor() => MyStatic.PINK;

        #endregion
    }
}
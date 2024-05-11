﻿using UnityEngine;
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

        // キャラクターが設定されない時に使われるメッセージボックスのタイプ
        const BoxType DefaultType = BoxType.Default;

        protected override async UniTask EnterAsync()
        {
            await SayAsync(storyText);
        }

        protected virtual async UniTask SayAsync(string text, string characterName = null, float boxShowTime = 0f)
        {
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
            if (msgBox.gameObject.activeInHierarchy == false)
            {
                await msgBox.ShowFadeAsync(boxShowTime, CallStatus.Token);
            }
            var charaName = string.IsNullOrEmpty(characterName) ? 
                character.CharacterName :
                characterName;
            await msgBox.Writer.WriteAsync(character, charaName, text, CallStatus.Token);
            await msgBox.Input.WaitInput(token: CallStatus.Token);
        }

        #region For EditorWindow

        protected override string GetSummary()
        {
            if(string.IsNullOrEmpty(storyText))
            {
                return WarningText();
            }
            int index = storyText.IndexOf("\n");
            var charaName = character == null ? string.Empty : character.CharacterName;
            if (index == -1)
            {
                return $"{charaName} \"{storyText}\"";
            }
            else
            {
                return $"{charaName} \"{storyText.Remove(index)}\"";
            }
        }

        protected override Color GetCommandColor() => new Color32(235, 210, 225, 255);

        public override string CSVContent1
        {
            get => character == null ? null : character.CharacterName;
            set
            {
                var chara = CharacterData.GetCharacter(value);
                if (chara == null) return;
                character = chara;
            }
        }

        public override string CSVContent2
        {
            get => storyText;
            set => storyText = value;
        }

        #endregion
    }
}
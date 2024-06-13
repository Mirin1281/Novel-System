using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Novel.Command
{
    [AddTypeMenu("Portrait"), System.Serializable]
    public class Portrait : CommandBase
    {
        public enum ActionType
        {
            Show,
            Change,
            Clear,
            HideOn,
            HideOff,
        }

        [SerializeField] CharacterData character;
        [SerializeField] ActionType actionType = ActionType.Show;
        [SerializeField] Sprite portraitSprite;
        [SerializeField] PortraitPositionType positionType;
        [SerializeField] Vector2 overridePos;
        [SerializeField] float fadeTime = 0.3f;
        [SerializeField] bool isAwait = true;

        protected override async UniTask EnterAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            if (actionType == ActionType.Show)
            {
                if (isAwait)
                {
                    await ShowAsync(portrait);
                }
                else
                {
                    ShowAsync(portrait).Forget();
                }
            }
            else if(actionType == ActionType.Change)
            {
                portrait.SetSprite(portraitSprite);
            }
            else if(actionType == ActionType.Clear)
            {
                if(isAwait)
                {
                    await portrait.ClearFadeAsync(fadeTime, CallStatus.Token);
                }
                else
                {
                    portrait.ClearFadeAsync(fadeTime, CallStatus.Token).Forget();
                }
            }
            else if(actionType == ActionType.HideOn)
            {
                portrait.SetHide(true);
            }
            else if (actionType == ActionType.HideOff)
            {
                portrait.SetHide(false);
            }
        }

        async UniTask ShowAsync(Novel.Portrait portrait)
        {
            if(positionType == PortraitPositionType.Custom)
            {
                portrait.SetPos(overridePos);
            }
            else
            {
                portrait.SetPos(positionType);
            }
            
            portrait.SetSprite(portraitSprite);
            await portrait.ShowFadeAsync(fadeTime, CallStatus.Token);
        }


        protected override string GetSummary()
        {
            if (character == null)
            {
                return WarningText();
            }
            if(portraitSprite == null && (actionType == ActionType.Show || actionType == ActionType.Change))
            {
                return WarningText();
            }
            return $"{character.CharacterName} {actionType}: {fadeTime}s";
        }

        protected override Color GetCommandColor() => new Color32(213, 245, 215, 255);

        public override string CSVContent1
        {
            get => character == null ? "Null" : character.CharacterName;
            set
            {
                if (value == "Null")
                {
                    character = null;
                    return;
                }
                var chara = CharacterData.GetCharacter(value);
                if (chara == null) return;
                character = chara;
            }
        }
        public override string CSVContent2
        {
            get => actionType.ToString();
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    actionType = ActionType.Show;
                    return;
                }
                if(value.TryParseToEnum(out ActionType type) == false)
                {
                    actionType = ActionType.Show;
                    Debug.LogWarning("Portrait, ActionTypeの変換に失敗しました");
                    return;
                }
                actionType = type;
            }
        }
    }
}
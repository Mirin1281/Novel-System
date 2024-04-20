﻿using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Novel.Command
{
    [AddTypeMenu("Portrait"), System.Serializable]
    public class PortraitCommand : CommandBase
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
        [SerializeField] ActionType actionType;
        [SerializeField] Sprite portraitSprite;
        [SerializeField] PortraitPosType positionType;
        [SerializeField] Vector2 overridePos;
        [SerializeField] float fadeTime;
        [SerializeField] bool isAwait;

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

        async UniTask ShowAsync(Portrait portrait)
        {
            if(positionType == PortraitPosType.Custom)
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
            get => character.CharacterName;
            set
            {
                character = CharacterData.GetCharacter(value);
            }
        }
        public override string CSVContent2
        {
            get => actionType.ToString();
            set
            {
                if(value.TryParseToEnum(out ActionType type) == false)
                {
                    actionType = ActionType.Show;
                    Debug.LogWarning("Portrait, ActionTypeの変換に失敗しました");
                }
                actionType = type;
            }
        }
    }
}
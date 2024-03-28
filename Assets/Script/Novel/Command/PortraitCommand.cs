using UnityEngine;
using Cysharp.Threading.Tasks;

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
        [SerializeField] PortraitPositionType positionType;
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
                Change(portrait);
            }
            else if(actionType == ActionType.Clear)
            {
                if(isAwait)
                {
                    await ClearAsync(portrait);
                }
                else
                {
                    ClearAsync(portrait).Forget();
                }
            }
            else if(actionType == ActionType.HideOn)
            {
                HideOn(portrait);
            }
            else if (actionType == ActionType.HideOff)
            {
                HideOff(portrait);
            }
        }

        async UniTask ShowAsync(Portrait portrait)
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
            await portrait.ShowFadeAsync(fadeTime);
        }

        void Change(Portrait portrait)
        {
            portrait.SetSprite(portraitSprite);
        }

        async UniTask ClearAsync(Portrait portrait)
        {
            await portrait.ClearFadeAsync(fadeTime);
        }

        void HideOn(Portrait portrait)
        {
            portrait.HideOn();
        }

        void HideOff(Portrait portrait)
        {
            portrait.HideOff();
        }

        protected override string GetSummary()
        {
            if (character == null)
            {
                return WarningColorText();
            }
            return $"{character.CharacterName} {actionType}: {fadeTime}s";
        }

        protected override Color GetCommandColor() => new Color32(213, 245, 215, 255);
    }
}
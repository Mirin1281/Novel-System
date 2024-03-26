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
            if(actionType == ActionType.Show)
            {
                if (isAwait)
                {
                    await ShowAsync();
                }
                else
                {
                    ShowAsync().Forget();
                }
            }
            else if(actionType == ActionType.Change)
            {
                Change();
            }
            else if(actionType == ActionType.Clear)
            {
                if(isAwait)
                {
                    await ClearAsync();
                }
                else
                {
                    ClearAsync().Forget();
                }
            }
            else if(actionType == ActionType.HideOn)
            {
                HideOn();
            }
            else if (actionType == ActionType.HideOff)
            {
                HideOff();
            }
        }

        async UniTask ShowAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
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

        void Change()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            portrait.SetSprite(portraitSprite);
        }

        async UniTask ClearAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            await portrait.ClearFadeAsync(fadeTime);
        }

        void HideOn()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            portrait.HideOn();
        }

        void HideOff()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
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

        protected override Color GetCommandColor() => MyStatic.LIGHT_GREEN;
    }
}
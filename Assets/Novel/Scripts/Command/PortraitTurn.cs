using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu(nameof(PortraitTurn)), System.Serializable]
    public class PortraitTurn : CommandBase
    {
        [SerializeField, DropDownCharacter] CharacterData character;
        [SerializeField] float time = 0.3f;
        [SerializeField] bool isAwait;

        protected override async UniTask EnterAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            if (isAwait)
            {
                await portrait.TurnAsync(time, Token);
            }
            else
            {
                portrait.TurnAsync(time).Forget();
            }
        }

        protected override string GetSummary()
        {
            if (character == null)
            {
                return WarningText();
            }
            return $"{character.CharacterName} {time}s";
        }
    }
}
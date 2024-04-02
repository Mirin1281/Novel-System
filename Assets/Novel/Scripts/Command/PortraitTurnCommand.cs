using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("PortraitTurn"), System.Serializable]
    public class PortraitTurnCommand : CommandBase
    {
        [SerializeField] CharacterData character;
        [SerializeField] float time;
        [SerializeField] bool isAwait;

        protected override async UniTask EnterAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            if(isAwait)
            {
                await portrait.TurnAsync(time, CallStatus.Token);
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
                return WarningColorText();
            }
            return $"{character.CharacterName} {time}s";
        }
    }
}
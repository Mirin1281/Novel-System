using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu(nameof(MoveXPortrait)), System.Serializable]
    public class MoveXPortrait : CommandBase
    {
        public enum MoveType { Relative, Absolute }

        [SerializeField, DropDownCharacter] CharacterData character;
        [SerializeField] MoveType moveType;
        [SerializeField] float movePosX;
        [SerializeField] float time;
        [SerializeField] bool isAwait;

        protected override async UniTask EnterAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            if(isAwait)
            {
                await Move(portrait.PortraitImageTs);
            }
            else
            {
                Move(portrait.PortraitImageTs).Forget();
            }
        }

        async UniTask Move(Transform transform)
        {
            var startPos = transform.localPosition;
            var deltaX = moveType switch
            {
                MoveType.Absolute => movePosX - startPos.x,
                MoveType.Relative => movePosX,
                _ => throw new System.Exception()
            };
            float t = 0f;
            while (t < time)
            {
                transform.localPosition = startPos + new Vector3(t / time * deltaX, 0);
                t += Time.deltaTime;
                await UniTask.Yield(Token);
            }
            transform.localPosition = startPos + new Vector3(deltaX, 0);
        }

        protected override string GetSummary()
        {
            if (character == null) return WarningText();
            return character.CharacterName;
        }
    }
}


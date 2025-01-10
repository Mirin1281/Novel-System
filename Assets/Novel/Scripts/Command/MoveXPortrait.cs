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
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] bool isAwait;

        protected override async UniTask EnterAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            if (isAwait)
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
            var easing = new Easing(startPos.x, moveType == MoveType.Absolute ? movePosX : movePosX + startPos.x, time, easeType);
            float t = 0f;
            while (t < time)
            {
                transform.localPosition = new Vector3(easing.Ease(t), startPos.y);
                t += Time.deltaTime;
                await UniTask.Yield(Token);
            }
            transform.localPosition = new Vector3(easing.Ease(time), startPos.y);
        }

        protected override string GetSummary()
        {
            if (character == null) return WarningText();
            return character.CharacterName;
        }
    }
}


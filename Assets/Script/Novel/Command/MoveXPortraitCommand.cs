using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Novel.Command
{
    [AddTypeMenu("MoveXPortrait"), System.Serializable]
    public class MoveXPortraitCommand : CommandBase
    {
        enum MoveType { Relative, Absolute }

        [SerializeField] CharacterData character;
        [SerializeField] float movePosX;
        [SerializeField] MoveType moveType;
        [SerializeField] Ease ease;
        [SerializeField] float time;
        [SerializeField] bool isAwait;

        protected override async UniTask EnterAsync()
        {
            var portrait = PortraitManager.Instance.CreateIfNotingPortrait(character.PortraitType);
            var portTs = portrait.PortraitImage.transform;

            if(isAwait)
            {
                if(moveType == MoveType.Relative)
                {
                    await portTs.DOLocalMoveX(movePosX, time)
                        .SetRelative(true).SetEase(ease).WithCancellation(CallStatus.Token);
                }
                else if(moveType == MoveType.Absolute)
                {
                    await portTs.DOLocalMoveX(movePosX, time).SetEase(ease).WithCancellation(CallStatus.Token);
                }
            }
            else
            {
                if (moveType == MoveType.Relative)
                {
                    _ = portTs.DOLocalMoveX(movePosX, time)
                        .SetRelative(true).SetEase(ease).WithCancellation(CallStatus.Token);
                }
                else if (moveType == MoveType.Absolute)
                {
                    _ = portTs.DOLocalMoveX(movePosX, time).SetEase(ease).WithCancellation(CallStatus.Token);
                }
            }
        }
    }
}


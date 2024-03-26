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
            if(isAwait)
            {
                if(moveType == MoveType.Relative)
                {
                    await portrait.PortraitImage.transform.DOLocalMoveX(movePosX, time)
                        .SetRelative(true).SetEase(ease);
                }
                else if(moveType == MoveType.Absolute)
                {
                    await portrait.PortraitImage.transform.DOLocalMoveX(movePosX, time)
                        .SetEase(ease);
                }
            }
            else
            {
                if (moveType == MoveType.Relative)
                {
                    _ = portrait.PortraitImage.transform.DOLocalMoveX(movePosX, time)
                        .SetRelative(true).SetEase(ease);
                }
                else if (moveType == MoveType.Absolute)
                {
                    _ = portrait.PortraitImage.transform.DOLocalMoveX(movePosX, time)
                        .SetEase(ease);
                }
            }
            return;
        }
    }
}


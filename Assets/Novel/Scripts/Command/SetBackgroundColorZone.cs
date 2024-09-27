using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("SetBackColorZone"), System.Serializable]
    public class SetBackgroundColorZone : CommandBase, IZoneCommand
    {
        [SerializeField] Color color;

        protected override async UniTask EnterAsync()
        {
            SetBackgroundColor(color);
            await UniTask.CompletedTask;
        }

        void IZoneCommand.CallIfInZone()
        {
            SetBackgroundColor(color);
        }

        void SetBackgroundColor(Color color)
        {
            Camera.main.backgroundColor = color;
        }

        protected override Color GetCommandColor() => color;
    }
}
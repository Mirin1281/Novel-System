using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("SetBackgroundColorZone"), System.Serializable]
    public class SetBackgroundColorZoneCommand : CommandBase, IZoneCommand
    {
        [SerializeField] Color color;

        protected override async UniTask EnterAsync()
        {
            Call();
            await UniTask.CompletedTask;
        }

        public void Call()
        {
            Camera.main.backgroundColor = color;
        }
    }
}
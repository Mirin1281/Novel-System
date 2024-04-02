using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("BackColorZone"), System.Serializable]
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

        protected override string GetName() => "BackColorZone";
    }
}
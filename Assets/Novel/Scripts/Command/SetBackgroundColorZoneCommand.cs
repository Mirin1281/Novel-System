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
            SetBackgroundColor(color);
            await UniTask.CompletedTask;
        }

        void IZoneCommand.CallZone()
        {
            SetBackgroundColor(color);
        }

        void SetBackgroundColor(Color color)
        {
            Camera.main.backgroundColor = color;
        }


        protected override string GetCommandInfo()
            => "Zoneコマンドの説明のために存在するコマンドです";
    }
}
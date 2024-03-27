using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("FlowJump"), System.Serializable]
    public class FlowJumpCommand : CommandBase
    {
        [SerializeField] int jumpCommandIndex;

        protected override async UniTask EnterAsync()
        {
            CalledFlowchart.Stop(FlowchartStopType.Single);
            UniTask.Void(async () =>
            {
                await UniTask.DelayFrame(1);
                CalledFlowchart.ExecuteAsync(jumpCommandIndex).Forget();
            });
            await UniTask.CompletedTask;
            return;
        }
    }
}

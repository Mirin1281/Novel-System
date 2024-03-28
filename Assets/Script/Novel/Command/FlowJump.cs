using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace Novel.Command
{
    [AddTypeMenu("FlowJump"), System.Serializable]
    public class FlowJumpCommand : CommandBase
    {
        enum JumpType
        {
            [InspectorName("絶対パス")] Absolute,
            [InspectorName("このコマンドよりN個上")] UpRelative,
            [InspectorName("このコマンドよりN個下")] DownRelative,
        }

        [SerializeField] int jumpIndex;
        [SerializeField] JumpType jumpType;

        protected override async UniTask EnterAsync()
        {
            ParentFlowchart.Stop(FlowchartStopType.Single);
            UniTask.Void(async () =>
            {
                await UniTask.DelayFrame(1);
                if(jumpType == JumpType.Absolute)
                {
                    ParentFlowchart.ExecuteAsync(jumpIndex).Forget();
                }
                else if(jumpType == JumpType.UpRelative)
                {
                    ParentFlowchart.ExecuteAsync(Index - jumpIndex).Forget();
                }
                else if (jumpType == JumpType.DownRelative)
                {
                    ParentFlowchart.ExecuteAsync(Index + jumpIndex).Forget();
                }
            });
            await UniTask.CompletedTask;
            return;
        }

        protected override string GetSummary()
        {
            int index = jumpType switch
            {
                JumpType.Absolute => jumpIndex,
                JumpType.UpRelative => Index - jumpIndex,
                JumpType.DownRelative => Index + jumpIndex,
                _ => throw new System.Exception()
            };
            var cmds = ParentFlowchart.GetCommandBaseList().ToArray();
            if (index < 0 || index >= cmds.Length || index == Index) return WarningColorText();
            var cmd = cmds[index];
            return $"To {ShapeCommandName(cmd)}";
        }
    }
}

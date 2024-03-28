using UnityEngine;
using Cysharp.Threading.Tasks;

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
            int index = jumpType switch
            {
                JumpType.Absolute => jumpIndex,
                JumpType.UpRelative => Index - jumpIndex,
                JumpType.DownRelative => Index + jumpIndex,
                _ => throw new System.Exception()
            };
            UniTask.Void(async () =>
            {
                await UniTask.DelayFrame(1);
                ParentFlowchart.ExecuteAsync(index).Forget();
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
            
            var cmdDataList = ParentFlowchart.GetCommandDataList();
            if (index < 0 || index >= cmdDataList.Count || index == Index) return WarningColorText();
            var cmd = cmdDataList[index].GetCommandBase();
            if(cmd == null || cmdDataList[index].Enabled == false) return WarningColorText();
            return $"To [{GetName(cmd)}]";
        }
    }
}

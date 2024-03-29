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
            ExecuteSelf(index, CallStatus).Forget();
            await UniTask.CompletedTask;
            return;
        }

        async UniTask ExecuteSelf(int index, FlowchartCallStatus callStatus)
        {
            await MyStatic.WaitFrame(1, callStatus.Token);
            ParentFlowchart.ExecuteAsync(index, callStatus).Forget();
        }


        protected override string GetName()
        {
            return "Jump";
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
            
            var cmdDataList = ParentFlowchart.GetReadOnlyCommandDataList();
            if (index < 0 || index >= cmdDataList.Count || index == Index) return WarningColorText();
            var cmd = cmdDataList[index].GetCommandBase();
            if(cmd == null || cmdDataList[index].Enabled == false) return WarningColorText();
            return $"<color=#000080>→ [ {GetName(cmd)} ]</color>";
        }
    }
}

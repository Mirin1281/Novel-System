using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("LoopEnd"), System.Serializable]
    public class LoopEnd : CommandBase
    {
        protected override async UniTask EnterAsync()
        {
            await UniTask.CompletedTask;

            var commandList = ParentFlowchart.GetReadOnlyCommandDataList();
            for(int i = Index; i >= 0; i--)
            {
                if(commandList[i].Enabled && commandList[i].GetCommandBase() is LoopStart loopStart)
                {
                    if(loopStart.IsBreak()) break;
                    ParentFlowchart.SetIndex(i, true);
                }
            }
        }
    }
}

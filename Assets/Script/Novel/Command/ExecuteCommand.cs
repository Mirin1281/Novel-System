using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("FlowExcute"), System.Serializable]
    public class ExcuteCommand : CommandBase
    {
        [SerializeField] FlowchartExecutor flowchartExecutor;
        [SerializeField] FlowchartData flowchartData;
        [SerializeField] int commandIndex;
        [SerializeField, Tooltip("false時は直列でFlowchartを呼びます\n" +
            "true時は呼び出したFlowchartが抜けるまで待ってから次のコマンドへ進みます")]
        bool isAwaitNest;

        protected override async UniTask EnterAsync()
        {
            if(flowchartExecutor != null)
            {
                if (isAwaitNest)
                {
                    await flowchartExecutor.ExecuteAsync(commandIndex, isNest: true);
                }
                else
                {
                    flowchartExecutor.Execute(commandIndex);
                    CalledFlowchart.Stop();
                }
            }
            if (isAwaitNest)
            {
                await flowchartData.ExecuteAsync(commandIndex, isNest: true);
            }
            else
            {
                flowchartData.Execute(commandIndex);
                CalledFlowchart.Stop();
            }
        }

        protected override string GetSummary()
        {
            if(flowchartExecutor == null && flowchartData == null)
            {
                return WarningColorText();
            }
            return isAwaitNest ? "Nest" : "NonNest";
        }
    }
}

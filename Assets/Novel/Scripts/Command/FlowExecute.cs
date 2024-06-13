using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("FlowExecute"), System.Serializable]
    public class FlowExecute : CommandBase
    {
        public enum FlowchartType
        {
            Executor,
            Data,
        }

        [SerializeField] FlowchartType flowchartType;
        [SerializeField] FlowchartExecutor flowchartExecutor;
        [SerializeField] FlowchartData flowchartData;
        [SerializeField] int commandIndex;
        [SerializeField, Tooltip(
            "false時は直列でFlowchartを呼びます\n" +
            "true時は呼び出したFlowchartが抜けるまで待ってから次のコマンドへ進みます")]
        bool isAwaitNest;

        protected override async UniTask EnterAsync()
        {
            Flowchart flowchart = flowchartType switch
            {
                FlowchartType.Executor => flowchartExecutor.Flowchart,
                FlowchartType.Data => flowchartData.Flowchart,
                _ => throw new System.Exception()
            };

            var isAwait = (isAwaitNest == false && CallStatus != null && CallStatus.IsNestCalled) || isAwaitNest;
            FlowchartCallStatus status = new(CallStatus.Token, CallStatus.Cts, isAwait);
            
            await flowchart.ExecuteAsync(commandIndex, status);
            if (isAwaitNest == false)
            {
                ParentFlowchart.Stop(Flowchart.StopType.Single);
            }
        }

        protected override string GetSummary()
        {
            string objectName = string.Empty;
            if(flowchartType == FlowchartType.Executor)
            {
                if (flowchartExecutor == null) return WarningText();
                objectName = flowchartExecutor.name;
            }
            else if(flowchartType == FlowchartType.Data)
            {
                if (flowchartData == null) return WarningText();
                objectName = flowchartData.name;
            }
            var nest = isAwaitNest ? "Nest" : string.Empty;
            return $"{objectName}   {nest}";
        }
    }
}

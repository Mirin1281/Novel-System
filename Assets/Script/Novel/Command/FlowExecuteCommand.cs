using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;

namespace Novel.Command
{
    [MovedFrom(false, null, null, "FlowExcuteCommand")]
    [AddTypeMenu("FlowExecute"), System.Serializable]
    public class FlowExecuteCommand : CommandBase
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
            IFlowchart flowchart = flowchartType switch
            {
                FlowchartType.Executor => flowchartExecutor,
                FlowchartType.Data => flowchartData,
                _ => throw new System.Exception()
            };

            FlowchartCallStatus status = new(CallStatus.Token, CallStatus.Cts, isAwaitNest);
            if (isAwaitNest)
            {
                await flowchart.ExecuteAsync(commandIndex, status);
            }
            else
            {
                flowchart.ExecuteAsync(commandIndex, status).Forget();
                ParentFlowchart.Stop(FlowchartStopType.Single);
            }
        }

        protected override string GetSummary()
        {
            string objectName = string.Empty;
            if(flowchartType == FlowchartType.Executor)
            {
                if (flowchartExecutor == null) return WarningColorText();
                objectName = flowchartExecutor.name;
            }
            else if(flowchartType == FlowchartType.Data)
            {
                if (flowchartData == null) return WarningColorText();
                objectName = flowchartData.name;
            }
            var nest = isAwaitNest ? "Nest" : "No Nest";
            return $"{objectName}   {nest}";
        }
    }
}

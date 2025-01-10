using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu(nameof(FlowExecute)), System.Serializable]
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
            "false時は次のFlowchartを呼びっぱなしにします\n" +
            "true時は呼び出したFlowchartが終了するまで待ってから次のコマンドへ進みます")]
        bool isAwaitNest;

        protected override async UniTask EnterAsync()
        {
            Flowchart flowchart = flowchartType switch
            {
                FlowchartType.Executor => flowchartExecutor.Flowchart,
                FlowchartType.Data => flowchartData.Flowchart,
                _ => throw new System.Exception()
            };

            if (isAwaitNest)
            {
                FlowchartCallStatus status = new(ParentFlowchart.CallStatus.Cts, existWaitOthers: true);
                await flowchart.ExecuteAsync(commandIndex, status);
            }
            else
            {
                FlowchartCallStatus status = new(ParentFlowchart.CallStatus.Cts, ParentFlowchart.CallStatus.ExistWaitOthers);
                flowchart.ExecuteAsync(commandIndex, status).Forget();
                ParentFlowchart.Stop(Flowchart.StopType.Single, isClearUI: false);
            }
        }

        protected override string GetSummary()
        {
            string objectName = string.Empty;
            if (flowchartType == FlowchartType.Executor)
            {
                if (flowchartExecutor == null) return WarningText();
                objectName = flowchartExecutor.name;
            }
            else if (flowchartType == FlowchartType.Data)
            {
                if (flowchartData == null) return WarningText();
                objectName = flowchartData.name;
            }
            string awaitStr = isAwaitNest ? "await  " : string.Empty;
            return $"{awaitStr}{objectName}";
        }
    }
}

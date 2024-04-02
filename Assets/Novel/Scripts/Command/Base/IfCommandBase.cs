using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    public abstract class IfCommandBase : CommandBase
    {
        [SerializeField] FlowchartExecutor flowchart;
        [SerializeField] FlowchartData flowchartData;
        [SerializeField, Tooltip("false時はフラグが真のときに下へ進みます")]
        bool isReverse;

        protected abstract bool IsMeet();

        protected override async UniTask EnterAsync()
        {
            if (IsMeet() == isReverse)
            {
                if(flowchart != null)
                {
                    flowchart.ExecuteAsync().Forget();
                }
                else
                {
                    flowchartData.ExecuteAsync().Forget();
                }
                
                ParentFlowchart.Stop(FlowchartStopType.Single);
                await UniTask.CompletedTask;
            }
        }
    }
}
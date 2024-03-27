using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("FlowStop"), System.Serializable]
    public class FlowStopCommand : CommandBase
    {
        [SerializeField] FlowchartStopType stopType;
        [SerializeField] bool hideMsgBoxes = true;
        [SerializeField] bool hidePortraits = true;

        protected override async UniTask EnterAsync()
        {
            CalledFlowchart.Stop(stopType);

            try
            {
                CallStatus.Token.ThrowIfCancellationRequested();
            }
            catch
            {
                ClearFadeUIIfSet();
                throw;
            }
            
            if (CallStatus.IsNestCalled == false)
            {
                ClearFadeUIIfSet();
            }
            await UniTask.CompletedTask;
        }

        void ClearFadeUIIfSet()
        {
            if (hideMsgBoxes)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
            }
            if (hidePortraits)
            {
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
        }
    }
}

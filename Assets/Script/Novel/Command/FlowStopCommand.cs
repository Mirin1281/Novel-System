using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("FlowStop"), System.Serializable]
    public class FlowStopCommand : CommandBase
    {
        [SerializeField] bool hideMsgBoxes = true;
        [SerializeField] bool hidePortraits = true;

        protected override async UniTask EnterAsync()
        {
            CalledFlowchart.Stop();
            if(hideMsgBoxes)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
            }
            if(hidePortraits)
            {
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            await UniTask.CompletedTask;
            return;
        }
    }
}

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
                MessageBoxManager.Instance.AllFadeOutAsync().Forget();
            }
            if(hidePortraits)
            {
                PortraitManager.Instance.AllFadeOutAsync().Forget();
            }
            await UniTask.CompletedTask;
            return;
        }
    }
}

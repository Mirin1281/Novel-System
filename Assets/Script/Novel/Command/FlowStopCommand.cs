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
        [SerializeField] bool checkLog = true;

        protected override async UniTask EnterAsync()
        {
#if UNITY_EDITOR
            if(checkLog)
            {
                if (CallStatus.IsNestCalled == false && stopType == FlowchartStopType.All)
                {
                    Debug.LogWarning("入れ子で呼び出していません！");
                }
                Debug.Log($"入れ子: {CallStatus.IsNestCalled}, ストップ: {stopType}");
            }
#endif

            ParentFlowchart.Stop(stopType);

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


        protected override string GetName()
        {
            return "Stop";
        }

        protected override string GetSummary()
        {
            return stopType.ToString();
        }
    }
}

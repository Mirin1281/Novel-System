using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("2Menu"), System.Serializable]
    public class TwoMenuCommand : CommandBase
    {
        public enum ActionType
        {
            [InspectorName("上 → 1個下, 下 → 2個下へ飛ぶ")] Jump,
            [InspectorName("フラグをセットする(上がTrue)")] SetFlag,

        }
        [SerializeField] string topSelectionText;
        [SerializeField] string bottomSelectionText;
        [SerializeField] ActionType actionType;
        [SerializeField] bool isReverse;
        [SerializeField] FlagKey_Bool flagKey;

        protected override async UniTask EnterAsync()
        {
            int clickedButtonIndex = await MenuManager.Instance.ShowAndWaitButtonClick(
                CallStatus.Token,
                new string[] {topSelectionText, bottomSelectionText});
            bool select = clickedButtonIndex == 0 ^ isReverse;

            if(actionType == ActionType.Jump)
            {
                ParentFlowchart.Stop(FlowchartStopType.Single);
                // Reverseを考えない場合、上を選んだ
                if (select)
                {
                    ExecuteSelf(Index + 1, CallStatus).Forget();
                }
                else
                {
                    ExecuteSelf(Index + 2, CallStatus).Forget();
                }
            }
            else if (actionType == ActionType.SetFlag)
            {
                FlagManager.SetFlagValue(flagKey, select);
            }
        }

        async UniTask ExecuteSelf(int index, FlowchartCallStatus callStatus)
        {
            await MyStatic.WaitFrame(1, callStatus.Token);
            ParentFlowchart.ExecuteAsync(index, callStatus).Forget();
        }


        protected override string GetName()
        {
            return "2Menu";
        }
    }
}

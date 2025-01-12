using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("2Menu"), System.Serializable]
    public class TwoMenu : CommandBase
    {
        public enum ActionType
        {
            [InspectorName("何もしない")] None,
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
                Token, new string[] { topSelectionText, bottomSelectionText });
            bool select = clickedButtonIndex == 0 ^ isReverse;

            if (actionType == ActionType.Jump)
            {
                int index;
                // Reverseを考えない場合、上を選んだ
                if (select)
                {
                    index = Index + 1;
                }
                else
                {
                    index = Index + 2;
                }
                ParentFlowchart.SetIndex(index, true);
            }
            else if (actionType == ActionType.SetFlag)
            {
                FlagManager.SetFlagValue(flagKey, select);
            }
        }
    }
}

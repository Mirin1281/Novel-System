using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("2Menu"), System.Serializable]
    public class TwoMenuCommand : CommandBase
    {
        [SerializeField] string topSelectionText;
        [SerializeField] string bottomSelectionText;
        [SerializeField] Menu menu;

        protected override async UniTask EnterAsync()
        {
            int clickedButtonIndex = await menu.ShowAndWaitButtonClick(
                new string[] {topSelectionText, bottomSelectionText});
            if(clickedButtonIndex == 0)
            {
                Debug.Log("上を選んだ");
            }
            else if(clickedButtonIndex == 1)
            {
                Debug.Log("下を選んだ");
            }
            return;
        }
    }
}

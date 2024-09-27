using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu(nameof(MessageBox)), System.Serializable]
    public class MessageBox : CommandBase
    {
        enum ShowType
        {
            Show,
            ClearAll,
        }

        [SerializeField] ShowType showType;
        [SerializeField] BoxType boxType;
        [SerializeField] float time = 0.3f;

        protected override async UniTask EnterAsync()
        {
            if(showType == ShowType.Show)
            {
                var box = MessageBoxManager.Instance.CreateIfNotingBox(boxType);
                await box.ShowFadeAsync(time, Token);
            }
            else if(showType == ShowType.ClearAll)
            {
                await MessageBoxManager.Instance.AllClearFadeAsync(time, Token);
            }
        }

        protected override string GetSummary()
        {
            return showType.ToString();
        }
    }
}
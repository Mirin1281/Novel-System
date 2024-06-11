using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;

namespace Novel.Command
{
    //[MovedFrom(false, null, null, "SetMessageBoxCommand")]
    [AddTypeMenu("MessageBox"), System.Serializable]
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
                await box.ShowFadeAsync(time, CallStatus.Token);
            }
            else if(showType == ShowType.ClearAll)
            {
                await MessageBoxManager.Instance.AllClearFadeAsync(time, CallStatus.Token);
            }
        }

        protected override string GetSummary()
        {
            return showType.ToString();
        }
    }
}
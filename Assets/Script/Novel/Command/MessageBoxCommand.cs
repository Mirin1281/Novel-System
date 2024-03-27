using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;

namespace Novel.Command
{
    //[MovedFrom(false, null, null, "SetMessageBoxCommand")]
    [AddTypeMenu("MessageBox"), System.Serializable]
    public class MessageBoxCommand : CommandBase
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
                await box.ShowFadeAsync(time);
            }
            else if(showType == ShowType.ClearAll)
            {
                await MessageBoxManager.Instance.AllClearFadeAsync(time);
            }
        }

        protected override string GetSummary()
        {
            return showType.ToString();
        }
    }
}
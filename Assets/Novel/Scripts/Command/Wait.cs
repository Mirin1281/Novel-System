using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Wait"), System.Serializable]
    public class Wait : CommandBase
    {
        [SerializeField] float waitSeconds;

        protected override async UniTask EnterAsync()
        {
            await Novel.Wait.Seconds(waitSeconds, CallStatus.Token);
        }
    }
}
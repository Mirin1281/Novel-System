using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu(nameof(Wait)), System.Serializable]
    public class Wait : CommandBase
    {
        [SerializeField] float waitSeconds;

        protected override async UniTask EnterAsync()
        {
            await AsyncUtility.Seconds(waitSeconds, Token);
        }
    }
}
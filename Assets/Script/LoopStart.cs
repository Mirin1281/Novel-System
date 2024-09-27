using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("LoopStart"), System.Serializable]
    public class LoopStart : CommandBase
    {
        [Header("多重ループには対応していません")]
        [SerializeField] int loopCount = 1;
        [SerializeField] bool useFlagKey;
        [SerializeField] FlagKey_Int countKey;
        [SerializeField] bool isStart1 = true;
        bool isEntered;
        int count = 0;

        protected override async UniTask EnterAsync()
        {
            await UniTask.CompletedTask;
            if(useFlagKey)
            {
                if(FlagManager.TryGetFlagValue(countKey, out int value))
                {
                    FlagManager.SetFlagValue(countKey, value + 1);
                }
                else
                {
                    FlagManager.SetFlagValue(countKey, isStart1 ? 1 : 0);
                }
            }
            else
            {
                if(isEntered == false)
                {
                    count = isStart1 ? 0 : -1;
                }
                isEntered = true;
                count++;
            }
        }

        public bool IsBreak()
        {
            if(useFlagKey)
            {
                FlagManager.TryGetFlagValue(countKey, out int value);
                return (isStart1 ? loopCount : loopCount - 1) <= value;
            }
            else
            {
                return (isStart1 ? loopCount : loopCount - 1) <= count;
            }
        }


        protected override string GetSummary()
        {
            if(countKey == null)
            {
                return $"Count: {loopCount}";
            }
            return $"Count: {loopCount}   Key: {countKey.GetName()}";
        }
    }
}

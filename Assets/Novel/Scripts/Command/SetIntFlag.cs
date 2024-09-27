using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("SetFlag/SetInt"), System.Serializable]
    public class SetIntFlag : CommandBase
    {
        [SerializeField] FlagKey_Int flagKey;
        [SerializeField] int value; 

        protected override async UniTask EnterAsync()
        {
            FlagManager.SetFlagValue(flagKey, value);
            await UniTask.CompletedTask;
        }

        protected override string GetSummary()
        {
            if (flagKey == null)
            {
                return WarningText();
            }
            return $"{flagKey.GetName()} = {value}";
        }
    }
}

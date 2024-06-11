using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("SetFlag/SetIntFlag"), System.Serializable]
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

        protected override string GetCommandInfo()
        {
            if (flagKey != null)
            {
                return $"ñºëO: {flagKey.GetName()}\nê‡ñæ: {flagKey.Description}";
            }
            return null;
        }
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("SetFlag/SetIntFlag"), System.Serializable]
    public class SetIntFlagCommand : CommandBase
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
            if (flagKey.name.Contains("int") == false)
            {
                return WarningText("FlagName don't contains \"int\"");
            }
            return $"{flagKey.name} = {value}";
        }

        protected override string GetCommandInfo()
        {
            if (flagKey != null)
            {
                return flagKey.Description;
            }
            return null;
        }
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("SetFlag/SetBoolFlag"), System.Serializable]
    public class SetBoolFlag : CommandBase
    {
        [SerializeField] FlagKey_Bool flagKey;
        [SerializeField] bool isOn;

        protected override async UniTask EnterAsync()
        {
            FlagManager.SetFlagValue(flagKey, isOn);
            await UniTask.CompletedTask;
        }

        protected override string GetSummary()
        {
            if(flagKey == null)
            {
                return WarningText();
            }
            if (flagKey.name.Contains("bool") == false)
            {
                return WarningText("FlagName don't contains \"bool\"");
            }
            return flagKey.name;
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

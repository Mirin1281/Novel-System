using UnityEngine;

namespace Novel.Command
{
    [AddTypeMenu("If/CheckBoolFlag"), System.Serializable]
    public class IfBoolFlagCommand : IfCommandBase
    {
        [SerializeField] FlagKey_Bool flag;

        protected override bool IsMeet()
        {
            var(isContain, result) = FlagManager.GetFlagValue(flag);
            if(isContain == false)
            {
                return false;
            }
            return result;
        }

        protected override string GetCommandInfo()
        {
            if (flag != null)
            {
                return flag.Description;
            }
            return null;
        }
    }
}
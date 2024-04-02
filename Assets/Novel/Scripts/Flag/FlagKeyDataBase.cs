using UnityEngine;

namespace Novel
{
    public abstract class FlagKeyDataBase : ScriptableObject
    {
        [SerializeField]
        string overrideFlagName;

        [field: SerializeField, TextArea]
        public string Description { get; private set; } = "説明";

        public string GetName()
        {
            if(string.IsNullOrEmpty(overrideFlagName))
            {
                return name;
            }
            return overrideFlagName;
        }
    }

    public abstract class FlagKeyDataBase<T> : FlagKeyDataBase
    {
        
    }
}
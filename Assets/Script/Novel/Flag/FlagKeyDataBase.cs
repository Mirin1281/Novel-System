using UnityEngine;

namespace Novel
{
    public abstract class FlagKeyDataBase : ScriptableObject
    {
        [field: SerializeField, TextArea]
        public string Description { get; private set; } = "説明";
    }

    public abstract class FlagKeyDataBase<T> : FlagKeyDataBase
    {
        
    }
}
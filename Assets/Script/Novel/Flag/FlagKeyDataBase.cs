using UnityEngine;
#pragma warning disable 0414 // value is never used の警告を消すため

namespace Novel
{
    public abstract class FlagKeyDataBase : ScriptableObject
    {
        [SerializeField, TextArea] string description = "説明";
        public string Description => description;
    }

    public abstract class FlagKeyDataBase<T> : FlagKeyDataBase
    {
        
    }
}
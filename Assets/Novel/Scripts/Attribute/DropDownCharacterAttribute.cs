using UnityEngine;
using System;

namespace Novel
{
    /// <summary>
    /// キャラクターのリストからドロップダウンで選ぶことができます
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DropDownCharacterAttribute : PropertyAttribute
    {
        public readonly string FieldName;

        public DropDownCharacterAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
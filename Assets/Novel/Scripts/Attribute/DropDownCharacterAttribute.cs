using UnityEngine;
using System;

namespace Novel
{
    /// <summary>
    /// キャラクターのリストからドロップダウン式で選ぶことができます
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DropDownCharacterAttribute : PropertyAttribute
    {

    }
}
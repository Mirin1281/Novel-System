using UnityEngine;
using System;

namespace Novel
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DropDownCharacterAttribute : PropertyAttribute { }
}
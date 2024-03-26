using UnityEngine;

[System.AttributeUsage(
    System.AttributeTargets.Field,
    AllowMultiple = true,
    Inherited = false)]
public class ReadOnlyAttribute : PropertyAttribute
{

}
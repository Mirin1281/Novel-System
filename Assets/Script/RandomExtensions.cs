using UnityEngine;

public　static class RandomExtensions
{
    /// <summary>
    /// bool型の乱数を取得します
    /// </summary>
    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 0;
    }
}

using UnityEngine;

public�@static class RandomExtensions
{
    /// <summary>
    /// bool�^�̗������擾���܂�
    /// </summary>
    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 0;
    }
}

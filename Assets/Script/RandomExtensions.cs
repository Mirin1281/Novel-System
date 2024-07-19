using UnityEngine;

public@static class RandomExtensions
{
    /// <summary>
    /// boolŒ^‚Ì—”‚ğæ“¾‚µ‚Ü‚·
    /// </summary>
    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 0;
    }
}
